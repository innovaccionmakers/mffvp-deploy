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
            var result = await orchestrator.CancelAsync(command.PortfolioId, command.ClosingDate, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancelación “controlada” de la request (se mapeará a 499 más arriba)
            logger.LogInformation(
                "CancelClosingCommand cancelado para Portafolio {PortfolioId}",
                command.PortfolioId);

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en CancelClosingCommand para Portafolio {PortfolioId}", command.PortfolioId);
            throw;
        }
    }
}