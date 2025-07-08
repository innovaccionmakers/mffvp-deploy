using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.YieldDetailCreation;
using Closing.Integrations.PreClosingSimulation.RunSimulation;
using System;

namespace Closing.Application.PreClosing.Services.Orchestation
{
    public class SimulationOrchestrator : ISimulationOrchestrator
    {
        IUnitOfWork unitOfWork;
        private readonly IProfitAndLossConsolidationService _profitAndLossConsolidationService;
        private readonly IYieldDetailCreationService _yieldDetailCreationService;
        public SimulationOrchestrator(
            IUnitOfWork unitOfWork,
            IProfitAndLossConsolidationService profitAndLossConsolidationService, 
            IYieldDetailCreationService yieldDetailCreationService)
        {
            this.unitOfWork = unitOfWork;
            _profitAndLossConsolidationService = profitAndLossConsolidationService;
            _yieldDetailCreationService = yieldDetailCreationService;
        }
        //public Task<SimulationResultDto> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken)

        //•Si ocurre un error en cualquier paso, lanza una excepción(throw o deja que la excepción natural ocurra).
        //•No atrapes excepciones dentro de RunSimulationAsync a menos que quieras manejarlas específicamente.Si las atrapas y no las relanzas, el handler no sabrá que hubo un error y la transacción podría cometerse incorrectamente.

        public async Task<bool> RunSimulationAsync(RunSimulationCommand parameters, CancellationToken cancellationToken)
        {
                //1.- Validar los parametros de entrada
                //TODO VALIDACIONES previas DE SIMULACION 

                //2.- Consolidacion PyG
                var pAndlSummary = await _profitAndLossConsolidationService.GetProfitAndLossSummaryAsync(
                    parameters.PortfolioId,
                    parameters.ClosingDate
                );

                var pAndlYieldDetails = _yieldDetailCreationService.PandLConceptSummaryToYieldDetails(pAndlSummary, parameters);

                _yieldDetailCreationService.CreateYieldDetailsAsync(pAndlYieldDetails,cancellationToken);

                //3.- Cálculo de Comisiones

                //4.- Conceptos Tesorería

                //5.- Consolidación Rendimientos
          
            return true;
          
        }
    }
}
