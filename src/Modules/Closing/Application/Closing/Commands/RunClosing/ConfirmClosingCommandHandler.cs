using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.RunClosing;

internal sealed class ConfirmClosingCommandHandler(
    IConfirmClosingOrchestrator orchestrator,
    IPostClosingServicesOrchestation postClosingServicesOrchestation,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmClosingCommandHandler> logger)
    : ICommandHandler<ConfirmClosingCommand, ConfirmClosingResult>
{
    public async Task<Result<ConfirmClosingResult>> Handle(ConfirmClosingCommand command, CancellationToken cancellationToken)
    {
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(command.ClosingDate);
        Result<ConfirmClosingResult> result;

        cancellationToken.ThrowIfCancellationRequested();
             

        // -------------------------
        // FASE 1: Cierre + persistencia base
        // -------------------------
        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await orchestrator.ConfirmAsync(command.PortfolioId, command.ClosingDate, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await unitOfWork.SaveChangesAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await transaction.CommitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                catch (Exception rollbackException)
                {
                    logger.LogWarning(
                        rollbackException,
                        "Rollback falló en ConfirmClosing FASE 1 tras cancelación para Portafolio {PortfolioId}",
                        command.PortfolioId);
                }

                logger.LogInformation(
                    "ConfirmClosing FASE 1 cancelado por token para Portafolio {PortfolioId}",
                    command.PortfolioId);

                throw;
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                catch (Exception rollbackException)
                {
                    logger.LogWarning(
                        rollbackException,
                        "Rollback falló en ConfirmClosing FASE 1 tras excepción para Portafolio {PortfolioId}",
                        command.PortfolioId);
                }

                logger.LogError(ex,
                    "ConfirmarCierre: Error en FASE 1 (Cierre - Distribución) para Portafolio {PortfolioId}", command.PortfolioId);
                throw;
            }
        }

        // -------------------------
        // FASE 2: Orquestación post-cierre 
        // -------------------------
        try
        {
            await postClosingServicesOrchestation.ExecuteAsync(command.PortfolioId, command.ClosingDate, CancellationToken.None);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "ConfirmarCierre: FASE 2 (post-cierre) cancelada por token para Portafolio {PortfolioId}, Fecha {ClosingDate}",
                command.PortfolioId,
                command.ClosingDate);

            throw;
        }
        catch (Exception ex)
        {
              logger.LogError(ex,
                "ConfirmarCierre: Error en FASE 2 (post-cierre) para Portafolio {PortfolioId}, Fecha {ClosingDate}",
                command.PortfolioId, command.ClosingDate);
              throw;
        }
        return result;
    }

}
