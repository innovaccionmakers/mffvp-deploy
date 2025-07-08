using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.CommissionCalculation;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
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
        public SimulationOrchestrator(
            IUnitOfWork unitOfWork,
            IProfitAndLossConsolidationService profitAndLossConsolidationService, 
            IYieldDetailCreationService yieldDetailCreationService,
            ICommissionCalculationService commissionCalculationService)
        {
            this.unitOfWork = unitOfWork;
            _profitAndLossConsolidationService = profitAndLossConsolidationService;
            _yieldDetailCreationService = yieldDetailCreationService;
            _commissionCalculationService = commissionCalculationService;
        }
        //public Task<SimulationResultDto> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken)

       public async Task<bool> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken)
        {

            // Validaciones pendientes...

            // 🧩 1. Tareas en paralelo
            // Consolidacion PyG
            var profitAndLossTask = Task.Run(async () =>
            {
                var summary = await _profitAndLossConsolidationService
                    .GetProfitAndLossSummaryAsync(parameters.PortfolioId, parameters.ClosingDate);

                var yieldDetails = _yieldDetailCreationService
                    .PandLConceptSummaryToYieldDetails(summary, parameters);
                
                await _yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, cancellationToken);
            });
            //Cálculo de Comisiones y Tesorería (comentado por falta de implementación)
            var feeTask = _commissionCalculationService.CalculateAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);
            //var treasuryTask = _treasuryConceptService.ExecuteAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);

            // Esperar a que las 3 tareas terminen
            // await Task.WhenAll(pygTask, feeTask, treasuryTask);
            await Task.WhenAll(profitAndLossTask, feeTask);

            // 🧩 2. Consolidación rendimientos final
            // await _yieldConsolidationService.ExecuteAsync(parameters.PortfolioId, parameters.ClosingDate, cancellationToken);

            return true;          
        }
    }
}
