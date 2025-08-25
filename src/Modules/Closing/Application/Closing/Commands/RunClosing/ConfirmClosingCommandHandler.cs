using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.RunClosing;

internal sealed class ConfirmClosingCommandHandler(
    IConfirmClosingOrchestrator orchestrator,
    IPostClosingEventsOrchestation postClosingEventsOrchestation,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmClosingCommandHandler> logger)
    : ICommandHandler<ConfirmClosingCommand, ConfirmClosingResult>
{
    public async Task<Result<ConfirmClosingResult>> Handle(ConfirmClosingCommand command, CancellationToken cancellationToken)
    {
        Result<ConfirmClosingResult> result;

        // -------------------------
        // FASE 1: Cierre + persistencia base
        // -------------------------
        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                result = await orchestrator.ConfirmAsync(command.PortfolioId, command.ClosingDate, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch (Exception rbEx)
                {
                    logger.LogWarning(rbEx,
                        "Rollback falló en ConfirmClosing para Portafolio {PortfolioId}", command.PortfolioId);
                }

                logger.LogError(ex,
                    "Error en FASE 1 (cierre) para Portafolio {PortfolioId}", command.PortfolioId);
                throw;
            }
        }

        // -------------------------
        // FASE 2: Orquestación post-cierre 
        // -------------------------
        try
        {
            await postClosingEventsOrchestation.ExecuteAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
        }
        catch (Exception ex)
        {
              logger.LogError(ex,
                "Error en FASE 2 (post-cierre) para Portafolio {PortfolioId}, Fecha {ClosingDate}",
                command.PortfolioId, command.ClosingDate);
            // Fallo total:
              throw;

            // Exito parcial:
            // return Result.Failure<ClosedResult>(new Error("POSTCLOSING_FAILED", "...", ErrorType.Unexpected));
        }

        return result;
    }
}
