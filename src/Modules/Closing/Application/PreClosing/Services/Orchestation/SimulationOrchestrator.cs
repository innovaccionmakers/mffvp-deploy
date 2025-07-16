using Associate.Domain.ConfigurationParameters;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.PreClosing.Services.CommissionCalculation;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Domain.PortfolioValuations;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using MediatR;
using System.Threading;


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
    private readonly IConfigurationParameterRepository _configurationParameterRepository;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;   
    IBusinessValidator<RunSimulationCommand> _businessValidator;

    public SimulationOrchestrator(
        IUnitOfWork unitOfWork,
        IProfitAndLossConsolidationService profitAndLossConsolidationService,
        ICommissionCalculationService commissionCalculationService,
        IMovementsConsolidationService movementsConsolidationService,
        IYieldDetailCreationService yieldDetailCreationService,
        YieldDetailBuilderService yieldDetailBuilderService,
        IYieldPersistenceService yieldPersistenceService,
        IConfigurationParameterRepository configurationParameterRepository,
        IPortfolioValuationRepository portfolioValuationRepository,
        IBusinessValidator<RunSimulationCommand> businessValidator)
    {
        _unitOfWork = unitOfWork;
        _profitAndLossConsolidationService = profitAndLossConsolidationService;
        _commissionCalculationService = commissionCalculationService;
        _movementsConsolidationService = movementsConsolidationService;
        _yieldDetailCreationService = yieldDetailCreationService;
        _yieldDetailBuilderService = yieldDetailBuilderService;
        _yieldPersistenceService = yieldPersistenceService;
        _configurationParameterRepository = configurationParameterRepository;
        _portfolioValuationRepository = portfolioValuationRepository;
        _businessValidator = businessValidator;

    }

    public async Task<Result<SimulatedYieldResult>> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var normalizedParams = NormalizeParameters(parameters);

        var validationResult = await ValidateBusinessRulesAsync(normalizedParams, ct);
        if (validationResult.IsFailure)
            return Result.Failure<SimulatedYieldResult>(validationResult.Error!);

        var isFirstClosingDay = await IsFirstClosingDayAsync(normalizedParams, ct);

        var localParameters = new RunSimulationParameters(
            normalizedParams.PortfolioId,
            normalizedParams.ClosingDate,
            isFirstClosingDay);

        var simulationResult = await ExecuteSimulationsAsync(localParameters, ct);
        if (simulationResult.IsFailure)
            return Result.Failure<SimulatedYieldResult>(simulationResult.Error!);

        await _unitOfWork.SaveChangesAsync(ct);

        var consolidationResult = await _yieldPersistenceService.ConsolidateAsync(localParameters, ct);
        return consolidationResult;
    }

    private static RunSimulationCommand NormalizeParameters(RunSimulationCommand parameters)
    {
        return parameters with
        {
            ClosingDate = DateTimeConverter.ToUtcDateTime(parameters.ClosingDate)
        };
    }

    private async Task<Result<Unit>> ValidateBusinessRulesAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var validation = await _businessValidator.ValidateAsync(parameters, ct);
        return validation.IsFailure ? Result.Failure<Unit>(validation.Error!) : Result.Success(Unit.Value);
    }

    private async Task<bool> IsFirstClosingDayAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        return !await _portfolioValuationRepository
            .ValuationExistsAsync(parameters.PortfolioId, parameters.ClosingDate.Date, ct);
    }

    private async Task<Result<Unit>> ExecuteSimulationsAsync(RunSimulationParameters parameters, CancellationToken ct)
    {
        var profitAndLossTask = ExecuteProfitAndLossSimulationAsync(parameters, ct);
        var commissionsTask = ExecuteCommissionSimulationAsync(parameters, ct);
        var treasuryTask = ExecuteTreasurySimulationAsync(parameters, ct);

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

    private async Task ExecuteProfitAndLossSimulationAsync(RunSimulationParameters parameters, CancellationToken ct)
    {
        if (parameters.IsFirstClosingDay) return;
        // If the first closing day, we do not calculate P&L
        var summary = await _profitAndLossConsolidationService
            .GetProfitAndLossSummaryAsync(parameters.PortfolioId, parameters.ClosingDate);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }

    private async Task ExecuteCommissionSimulationAsync(RunSimulationParameters parameters, CancellationToken ct)
    {
       // if (parameters.IsFirstClosingDay) return;
        // If the first closing day, we do not calculate commissions
        var summary = await _commissionCalculationService
            .CalculateAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }

    private async Task ExecuteTreasurySimulationAsync(RunSimulationParameters parameters, CancellationToken ct)
    {
        if (parameters.IsFirstClosingDay) return;
        // If the first closing day, we do not calculate treasury movements
        var summary = await _movementsConsolidationService
            .GetMovementsSummaryAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }
}
