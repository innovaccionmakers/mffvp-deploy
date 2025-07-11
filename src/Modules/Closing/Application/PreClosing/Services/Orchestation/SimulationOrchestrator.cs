using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.CommissionCalculation;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.YieldDetailCreation;
using Closing.Integrations.PreClosing.RunSimulation;

namespace Closing.Application.PreClosing.Services.Orchestation
{
    public class SimulationOrchestrator : ISimulationOrchestrator
    {
        IUnitOfWork unitOfWork;
        private readonly IProfitAndLossConsolidationService _profitAndLossConsolidationService;
        private readonly IYieldDetailCreationService _yieldDetailCreationService;
        private readonly ICommissionCalculationService _commissionCalculationService;
        private readonly IMovementsConsolidationService _movementsConsolidationService;
        public SimulationOrchestrator(
            IUnitOfWork unitOfWork,
            IProfitAndLossConsolidationService profitAndLossConsolidationService, 
            IYieldDetailCreationService yieldDetailCreationService,
            ICommissionCalculationService commissionCalculationService,
            IMovementsConsolidationService movementsConsolidationService)
        {
            this.unitOfWork = unitOfWork;
            _profitAndLossConsolidationService = profitAndLossConsolidationService;
            _yieldDetailCreationService = yieldDetailCreationService;
            _commissionCalculationService = commissionCalculationService;
            _movementsConsolidationService = movementsConsolidationService;
        }

        public async Task<bool> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
        {
            // TODO: Validaciones de entrada

            var profitAndLossTask = ExecuteProfitAndLossSimulationAsync(parameters, ct);
            var commissionsTask = ExecuteCommissionSimulationAsync(parameters, ct);
            var treasuryTask = ExecuteTreasurySimulationAsync(parameters, ct);

            await Task.WhenAll(profitAndLossTask, commissionsTask, treasuryTask);

            // TODO: Consolidación de rendimientos final
            // await _yieldConsolidationService.ExecuteAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

            return true;
        }
        private async Task ExecuteProfitAndLossSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
        {
            var summary = await _profitAndLossConsolidationService
                .GetProfitAndLossSummaryAsync(parameters.PortfolioId, parameters.ClosingDate);

            if (!summary.Any()) return;

            var yieldDetails = _yieldDetailCreationService
                .PandLConceptSummaryToYieldDetails(summary, parameters);

            await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
        }

        private async Task ExecuteCommissionSimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
        {
            var summary = await _commissionCalculationService
                .CalculateAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

            if (!summary.Any()) return;

            var yieldDetails = _yieldDetailCreationService
                .CommissionConceptSummaryToYieldDetails(summary, parameters);

            await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
        }

        private async Task ExecuteTreasurySimulationAsync(RunSimulationCommand parameters, CancellationToken ct)
        {
            var summary = await _movementsConsolidationService
                .GetMovementsSummaryAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

            if (!summary.Any()) return;

            var yieldDetails = _yieldDetailCreationService
                .TreasuryConceptSummaryToYieldDetails(summary, parameters); // 💡 Corrige aquí tu llamado faltante

            await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, ct);
        }

    }
}
