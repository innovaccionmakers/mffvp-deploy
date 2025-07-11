
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PreClosing.RunSimulation
{
    internal sealed class RunSimulationCommandHandler(
    ISimulationOrchestrator _simulationOrchestrator,
    IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    ILogger<RunSimulationCommandHandler> logger)
    : ICommandHandler<RunSimulationCommand, bool>
    {
        public async Task<Result<bool>> Handle(RunSimulationCommand command, CancellationToken cancellationToken)
        {

            var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                //var sw = Stopwatch.StartNew();
                await _simulationOrchestrator.RunSimulationAsync(command, cancellationToken);
                //sw.Stop();
                logger?.LogInformation("Simulación ejecutada en {ElapsedMilliseconds} ms para Portafolio {PortfolioId}");
                //sw.ElapsedMilliseconds, command.PortfolioId);

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger?.LogError(ex, "Error en RunSimulationCommand para Portafolio {PortfolioId} - Fecha {Date}",
                    command.PortfolioId, command.ClosingDate);
                throw;
            }

            return Result.Success(true, "La operación se realizó exitosamente.");
        }
    }
}
