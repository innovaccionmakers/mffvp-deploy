using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PreClosing.Services.Validation;
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
        : ICommandHandler<RunClosingCommand, ClosedResult>
{
    public async Task<Result<ClosedResult>> Handle(RunClosingCommand command, CancellationToken cancellationToken)
    {
        await using var transaction =
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {

            var result = await closingOrchestrator.PrepareAsync(command, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await cancelClosingOrchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
            logger?.LogError(ex, "Error en RunClosingCommand para Portafolio {PortfolioId} - Fecha {Date}",
                command.PortfolioId, command.ClosingDate);
            throw;
        }
    }
}
