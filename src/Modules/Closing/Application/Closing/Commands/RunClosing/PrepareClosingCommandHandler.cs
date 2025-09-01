using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Commands.RunClosing;

internal sealed class PrepareClosingCommandHandler(
        IPrepareClosingOrchestrator closingOrchestrator,
        ICancelClosingOrchestrator cancelClosingOrchestrator,
        IUnitOfWork unitOfWork,
        ILogger<PrepareClosingCommandHandler> logger
       )
        : ICommandHandler<PrepareClosingCommand, PrepareClosingResult>
{
    public async Task<Result<PrepareClosingResult>> Handle(PrepareClosingCommand command, CancellationToken cancellationToken)
    {

        cancellationToken.ThrowIfCancellationRequested();
        await using var transaction =
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await closingOrchestrator.PrepareAsync(command, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await cancelClosingOrchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
            logger?.LogError(ex, "Error en PrepareClosingCommand para Portafolio {PortfolioId} - Fecha {Date}",
                command.PortfolioId, command.ClosingDate);
            throw;
        }
    }
}
