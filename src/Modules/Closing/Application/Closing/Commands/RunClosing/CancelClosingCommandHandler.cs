
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Commands.RunClosing;

internal sealed class CancelClosingCommandHandler(
    ICancelClosingOrchestrator orchestrator,
    ILogger<CancelClosingCommandHandler> logger)
    : ICommandHandler<CancelClosingCommand>
{
    public async Task<Result> Handle(CancelClosingCommand command, CancellationToken ct)
    {
        try
        {
            var result = await orchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, ct);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en CancelClosingCommand para Portafolio {PortfolioId}", command.PortfolioId);
            throw;
        }
    }
}