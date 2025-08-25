
using Closing.Application.Abstractions.Data;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Closing.Application.PreClosing.RunSimulation;

internal sealed class RunSimulationCommandHandler(
ISimulationOrchestrator _simulationOrchestrator,
IUnitOfWork unitOfWork,
 ISimulationExecutionLock execLock,
ILogger<RunSimulationCommandHandler> logger)
: ICommandHandler<RunSimulationCommand, SimulatedYieldResult>
{
    public async Task<Result<SimulatedYieldResult>> Handle(
        RunSimulationCommand command, 
        CancellationToken cancellationToken = default)
    {
        using var scopeTotal = logger.BeginScope(new Dictionary<string, object?>
        {
            ["PortfolioId"] = command.PortfolioId,
            ["ClosingDate"] = command.ClosingDate.ToString("yyyy-MM-dd"),
            ["IsClosing"] = command.IsClosing
        });
        var outcome = "Success";
        var swTotal = Stopwatch.StartNew();
        var handle = await execLock.TryAcquireAsync(command.PortfolioId, command.ClosingDate, command.IsClosing, cancellationToken);
        if (handle is null)
        {
            outcome = "Rejected-Locked";
            swTotal.Stop();
            var msg = $"Ya existe una simulación en ejecución para el portafolio {command.PortfolioId} y fecha {command.ClosingDate:yyyy-MM-dd}.";
            logger.LogWarning("Simulación rechazada por lock: {Message}", msg);

            return Result.Failure<SimulatedYieldResult>(
                new Error("SIM001", msg, ErrorType.Failure));
        }
        using (handle)
        {

            var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {

                logger?.LogInformation("Iniciando simulación para Portafolio {PortfolioId} - Fecha {Date}", command.PortfolioId, command.ClosingDate);
                var result = await _simulationOrchestrator.RunSimulationAsync(command, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                swTotal.Stop();
                logger?.LogInformation("Simulación ejecutada para Portafolio {PortfolioId} - Fecha {Date} - Tiempo total: {ElapsedMs} ms", command.PortfolioId, command.ClosingDate, swTotal.ElapsedMilliseconds);
                return result;

            }
            catch (Exception ex)
            {
                outcome = "Failure";
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);

                logger?.LogInformation(ex, "Error en RunSimulationCommand para Portafolio {PortfolioId} - Fecha {Date}",
                    command.PortfolioId, command.ClosingDate);
                throw;
            }
            finally
            {
                swTotal.Stop();
                logger.LogInformation("Simulación terminada. Outcome={Outcome}. Tiempo total={ElapsedMs} ms",
                    outcome, swTotal.ElapsedMilliseconds);
            }
        }

       

    }
}
