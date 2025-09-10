using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Commission.Interfaces;
using Closing.Application.PreClosing.Services.ProfitAndLoss;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;

namespace Closing.Application.PreClosing.Services.Orchestation;

public class SimulationOrchestrator : ISimulationOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProfitAndLossConsolidationService _profitAndLossConsolidationService;
    private readonly ICommissionCalculationService _commissionCalculationService;
    private readonly IMovementsConsolidationService _movementsConsolidationService;
    private readonly IYieldDetailCreationService _yieldDetailCreationService;
    private readonly YieldDetailBuilderService _yieldDetailBuilderService;
    private readonly IYieldPersistenceService _yieldPersistenceService;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;
    IRunSimulationValidationReader _businessValidator;
    private readonly IYieldDetailRepository _yieldDetailRepository;
    private readonly IYieldRepository _yieldRepository;
    private readonly IPortfolioValidator _portfolioValidator;
    private readonly IWarningCollector _warnings;
    private readonly IPreclosingCleanupService _preclosingCleanupService;

    public SimulationOrchestrator(
        IUnitOfWork unitOfWork,
        IProfitAndLossConsolidationService profitAndLossConsolidationService,
        ICommissionCalculationService commissionCalculationService,
        IMovementsConsolidationService movementsConsolidationService,
        IYieldDetailCreationService yieldDetailCreationService,
        YieldDetailBuilderService yieldDetailBuilderService,
        IYieldPersistenceService yieldPersistenceService,
        IPortfolioValuationRepository portfolioValuationRepository,
        IRunSimulationValidationReader businessValidator,
        IYieldDetailRepository yieldDetailRepository,
        IYieldRepository yieldRepository,
        IPortfolioValidator portfolioValidator,
        IWarningCollector warningCollector,
         IPreclosingCleanupService preclosingCleanupService)
    {
        _unitOfWork = unitOfWork;
        _profitAndLossConsolidationService = profitAndLossConsolidationService;
        _commissionCalculationService = commissionCalculationService;
        _movementsConsolidationService = movementsConsolidationService;
        _yieldDetailCreationService = yieldDetailCreationService;
        _yieldDetailBuilderService = yieldDetailBuilderService;
        _yieldPersistenceService = yieldPersistenceService;
        _portfolioValuationRepository = portfolioValuationRepository;
        _businessValidator = businessValidator;
        _yieldDetailRepository = yieldDetailRepository;
        _yieldRepository = yieldRepository;
        _portfolioValidator = portfolioValidator;
        _warnings = warningCollector;
        _preclosingCleanupService = preclosingCleanupService;
    }
    public async Task<Result<SimulatedYieldResult>> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken)
    {
        var normalizedParams = NormalizeParameters(parameters);
        var isFirstClosingDay = false;

        if (!normalizedParams.IsClosing)  //Cuando es Cierre, se valida en el orquestador de cierre
        {
            var validation = await _businessValidator.ValidateAndDescribeAsync(normalizedParams, cancellationToken);
            if (validation.IsFailure)
                return Result.Failure<SimulatedYieldResult>(validation.Error!);
            isFirstClosingDay = validation.Value.IsFirstClosingDay;
        }else
        {
            var firstDayResult = await IsFirstClosingDayAsync(normalizedParams, cancellationToken);
            if (firstDayResult.IsFailure)
                return Result.Failure<SimulatedYieldResult>(firstDayResult.Error!);

            isFirstClosingDay = firstDayResult.Value;
        } 

        var localParameters = new RunSimulationParameters(
            normalizedParams.PortfolioId,
            normalizedParams.ClosingDate,
            normalizedParams.IsClosing,
            isFirstClosingDay);

        var simulationResult = await ExecuteSimulationsAsync(localParameters, cancellationToken);
        if (simulationResult.IsFailure)
            return Result.Failure<SimulatedYieldResult>(simulationResult.Error!);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var consolidationResult = await _yieldPersistenceService.ConsolidateAsync(localParameters, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var warnings = _warnings.GetAll();

        consolidationResult.HasWarnings = warnings.Any();
        consolidationResult.Warnings = warnings;

        return consolidationResult;
    }

    private static RunSimulationCommand NormalizeParameters(RunSimulationCommand parameters)
    {
        return parameters with
        {
            ClosingDate = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate)
        };
    }

    private async Task<Result<bool>> IsFirstClosingDayAsync(
        RunSimulationCommand parameters,
        CancellationToken cancellationToken)
    {
        var portfolioDataResult = await _portfolioValidator.GetPortfolioDataAsync(parameters.PortfolioId, cancellationToken);
        if (portfolioDataResult.IsFailure)
            return Result.Failure<bool>(portfolioDataResult.Error!);

        var portfolioData = portfolioDataResult.Value;

        var exists = await _portfolioValuationRepository
            .ExistsByPortfolioAndDateAsync(parameters.PortfolioId, portfolioData.CurrentDate, cancellationToken);

        return Result.Success(!exists);
    }

    private async Task<Result<Unit>> ExecuteSimulationsAsync(RunSimulationParameters parameters, CancellationToken cancellationToken)
    {
        await _preclosingCleanupService.CleanAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);

        var profitAndLossTask = ExecuteProfitAndLossSimulationAsync(parameters, cancellationToken);
        var commissionsTask = ExecuteCommissionSimulationAsync(parameters, cancellationToken);
        var treasuryTask = ExecuteTreasurySimulationAsync(parameters, cancellationToken);

        try
        {
            await Task.WhenAll(profitAndLossTask, commissionsTask, treasuryTask);
            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>(new Error("000",$"Error ejecutando simulaciones: {ex.Message}", ErrorType.Failure));
        }
    }

    private async Task ExecuteProfitAndLossSimulationAsync(RunSimulationParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.IsFirstClosingDay) return;

        var summary = await _profitAndLossConsolidationService
            .GetProfitAndLossSummaryAsync(parameters.PortfolioId, parameters.ClosingDate);

        if (summary is null || !summary.Any())
        {
            _warnings.Add(WarningCatalog.Adv001PygMissing());
            return;
        }

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, Yield.Constants.PersistenceMode.Immediate, cancellationToken);
    }

    private async Task ExecuteCommissionSimulationAsync(RunSimulationParameters parameters, CancellationToken cancellationToken)
    {
        var summary = await _commissionCalculationService
            .CalculateAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, Yield.Constants.PersistenceMode.Immediate, cancellationToken);
    }

    private async Task ExecuteTreasurySimulationAsync(RunSimulationParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.IsFirstClosingDay) return;

        var summary = await _movementsConsolidationService
            .GetMovementsSummaryAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, Yield.Constants.PersistenceMode.Immediate, cancellationToken);
    }
}
