using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.CommissionCalculation;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Integrations.PreClosing.RunSimulation;

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

    public SimulationOrchestrator(
        IUnitOfWork unitOfWork,
        IProfitAndLossConsolidationService profitAndLossConsolidationService,
        ICommissionCalculationService commissionCalculationService,
        IMovementsConsolidationService movementsConsolidationService,
        IYieldDetailCreationService yieldDetailCreationService,
        YieldDetailBuilderService yieldDetailBuilderService,
        IYieldPersistenceService yieldPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _profitAndLossConsolidationService = profitAndLossConsolidationService;
        _commissionCalculationService = commissionCalculationService;
        _movementsConsolidationService = movementsConsolidationService;
        _yieldDetailCreationService = yieldDetailCreationService;
        _yieldDetailBuilderService = yieldDetailBuilderService;
        _yieldPersistenceService = yieldPersistenceService;
    }

    public async Task<bool> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var profitAndLossTask = ExecuteProfitAndLossSimulationAsync(parameters, ct);
        var commissionsTask = ExecuteCommissionSimulationAsync(parameters, ct);
        var treasuryTask = ExecuteTreasurySimulationAsync(parameters, ct);

        await Task.WhenAll(profitAndLossTask, commissionsTask, treasuryTask);

        // TODO: Consolidación de rendimientos final
        await _yieldPersistenceService.ConsolidateAsync(parameters.PortfolioId, parameters.ClosingDate, false, ct);
        return true;
    }

    private async Task ExecuteProfitAndLossSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var summary = await _profitAndLossConsolidationService
            .GetProfitAndLossSummaryAsync(parameters.PortfolioId, parameters.ClosingDate);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }

    private async Task ExecuteCommissionSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var summary = await _commissionCalculationService
            .CalculateAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }

    private async Task ExecuteTreasurySimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var summary = await _movementsConsolidationService
            .GetMovementsSummaryAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

        if (!summary.Any()) return;

        var yieldDetails = _yieldDetailBuilderService
            .Build(summary, parameters);

        await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
    }
}
