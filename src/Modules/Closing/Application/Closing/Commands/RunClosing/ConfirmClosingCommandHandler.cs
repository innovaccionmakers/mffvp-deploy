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
    : ICommandHandler<ConfirmClosingCommand, ClosedResult>
{
    public async Task<Result<ClosedResult>> Handle(ConfirmClosingCommand command, CancellationToken cancellationToken)
    {
        Result<ClosedResult> result;

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
                    // Evita que un rollback fallido tape la excepción original
                    logger.LogWarning(rbEx,
                        "Rollback falló en ConfirmClosing para Portafolio {PortfolioId}", command.PortfolioId);
                }

                logger.LogError(ex,
                    "Error en FASE 1 (cierre) para Portafolio {PortfolioId}", command.PortfolioId);
                throw;
            }
        }

        // -------------------------
        // FASE 2: Orquestación post-cierre (sin transacción previa viva)
        // -------------------------
        try
        {
            await postClosingEventsOrchestation.ExecuteAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
        }
        catch (Exception ex)
        {
            // Aquí decides la política:
            // - Re-lanzar para que el endpoint falle y el cliente vea el error de post-cierre
            // - O registrar y devolver éxito parcial (si negocio lo permite) + disparar reintento
            logger.LogError(ex,
                "Error en FASE 2 (post-cierre) para Portafolio {PortfolioId}, Fecha {ClosingDate}",
                command.PortfolioId, command.ClosingDate);

            // Si necesitas que el endpoint falle:
            throw;

            // Si prefieres éxito parcial:
            // return Result.Failure<ClosedResult>(new Error("POSTCLOSING_FAILED", "...", ErrorType.Unexpected));
        }

        return result;
    }
}
