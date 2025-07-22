using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.RunClosing;

internal sealed class ConfirmClosingCommandHandler(
    IConfirmClosingOrchestrator orchestrator,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmClosingCommandHandler> logger)
    : ICommandHandler<RunClosingCommand, ClosedResult>
{
    public async Task<Result<ClosedResult>> Handle(RunClosingCommand command, CancellationToken ct)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var result = await orchestrator.ConfirmAsync(command.PortfolioId, command.ClosingDate, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error en ConfirmClosingCommand para Portafolio {PortfolioId}", command.PortfolioId);
            throw;
        }
    }
}
