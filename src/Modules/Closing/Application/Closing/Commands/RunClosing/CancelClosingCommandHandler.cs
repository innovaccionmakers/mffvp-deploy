using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Commands.RunClosing;

internal sealed class CancelClosingCommandHandler(
    ICancelClosingOrchestrator orchestrator,
    ILogger<CancelClosingCommandHandler> logger)
    : ICommandHandler<CancelClosingCommand, CancelClosingResult>
{
    public async Task<Result<CancelClosingResult>> Handle(CancelClosingCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await orchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en CancelClosingCommand para Portafolio {PortfolioId}", command.PortfolioId);
            throw;
        }
    }
}