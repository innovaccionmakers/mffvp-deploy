
using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Commands.RunClosing;

internal sealed class CancelClosingCommandHandler(
    ICancelClosingOrchestrator orchestrator,
   // IUnitOfWork unitOfWork,
    ILogger<CancelClosingCommandHandler> logger)
    : ICommandHandler<CancelClosingCommand, ClosedResult>
{
    public async Task<Result<ClosedResult>> Handle(CancelClosingCommand command, CancellationToken ct)
    {
        //var transaction = await unitOfWork.BeginTransactionAsync(ct);
        // el manejo de la transacción se ha movido al orchestrator
        try
        {
            var result = await orchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, ct);

            //await unitOfWork.SaveChangesAsync(ct);
            //await transaction.CommitAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            //await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error en CancelClosingCommand para Portafolio {PortfolioId}", command.PortfolioId);
            throw;
        }
    }
}