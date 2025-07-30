
using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PreClosing.RunSimulation
{
    internal sealed class RunSimulationCommandHandler(
    ISimulationOrchestrator _simulationOrchestrator,
    IUnitOfWork unitOfWork,
    ILogger<RunSimulationCommandHandler> logger)
    : ICommandHandler<RunSimulationCommand, SimulatedYieldResult>
    {
        public async Task<Result<SimulatedYieldResult>> Handle(RunSimulationCommand command, CancellationToken cancellationToken)
        {

            var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
             

                logger?.LogInformation("Iniciando simulación para Portafolio {PortfolioId} - Fecha {Date}",command.PortfolioId, command.ClosingDate);
                //var sw = Stopwatch.StartNew();
                var result = await _simulationOrchestrator.RunSimulationAsync(command, cancellationToken);
                //sw.Stop();
                logger?.LogInformation("Simulación ejecutada para Portafolio {PortfolioId} - Fecha {Date}", command.PortfolioId, command.ClosingDate);

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger?.LogError(ex, "Error en RunSimulationCommand para Portafolio {PortfolioId} - Fecha {Date}",
                    command.PortfolioId, command.ClosingDate);
                throw;
            }

        }
    }
}
