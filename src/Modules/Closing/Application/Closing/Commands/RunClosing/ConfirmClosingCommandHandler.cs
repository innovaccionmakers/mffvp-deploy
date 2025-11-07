using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Domain.PortfolioValuations;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.RunClosing;

internal sealed class ConfirmClosingCommandHandler(
    IConfirmClosingOrchestrator orchestrator,
    IPostClosingServicesOrchestation postClosingServicesOrchestation,
    IUnitOfWork unitOfWork,
    IPortfolioValidator portfolioValidator,
    IPortfolioValuationRepository portfolioValuationRepository,
    ILogger<ConfirmClosingCommandHandler> logger)
    : ICommandHandler<ConfirmClosingCommand, ConfirmClosingResult>
{
    public async Task<Result<ConfirmClosingResult>> Handle(ConfirmClosingCommand command, CancellationToken cancellationToken)
    {
        Result<ConfirmClosingResult> result;

        cancellationToken.ThrowIfCancellationRequested();

        var isFirstClosingDayResult = await IsFirstClosingDayAsync(command.PortfolioId, cancellationToken);
        if (isFirstClosingDayResult.IsFailure)
            return Result.Failure<ConfirmClosingResult>(isFirstClosingDayResult.Error);

        bool isFirstClosingDay = isFirstClosingDayResult.Value;

        // -------------------------
        // FASE 1: Cierre + persistencia base
        // -------------------------
        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await orchestrator.ConfirmAsync(command.PortfolioId, command.ClosingDate, isFirstClosingDay, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await unitOfWork.SaveChangesAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
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
                    "ConfirmarCierre: Error en FASE 1 (Cierre - Distribución) para Portafolio {PortfolioId}", command.PortfolioId);
                throw;
            }
        }

        // -------------------------
        // FASE 2: Orquestación post-cierre 
        // -------------------------
        try
        {
            await postClosingServicesOrchestation.ExecuteAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
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

    private async Task<Result<bool>> IsFirstClosingDayAsync(
        int portfolioId,
        CancellationToken cancellationToken)
    {
        var portfolioDataResult = await portfolioValidator.GetPortfolioDataAsync(portfolioId, cancellationToken);
        if (portfolioDataResult.IsFailure)
            return Result.Failure<bool>(portfolioDataResult.Error);

        var portfolioData = portfolioDataResult.Value;

        var exists = await portfolioValuationRepository
            .ExistsByPortfolioAndDateAsync(portfolioId, portfolioData.CurrentDate, cancellationToken);

        return Result.Success(!exists);
    }
}
