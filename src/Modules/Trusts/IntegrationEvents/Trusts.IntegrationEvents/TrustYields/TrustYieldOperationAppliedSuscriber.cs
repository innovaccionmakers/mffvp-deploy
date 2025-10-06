using DotNetCore.CAP;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.TrustOperations;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.IntegrationEvents.TrustYields;

public sealed class TrustYieldOperationAppliedSuscriber(ISender mediator, ILogger<TrustYieldOperationAppliedSuscriber> logger) : ICapSubscribe
{
    [CapSubscribe(nameof(TrustYieldOperationAppliedIntegrationEvent))]
    public async Task HandleAsync(TrustYieldOperationAppliedIntegrationEvent message,
                              [FromCap] CapHeader headers,
                              CancellationToken cancellationToken)
    {
        var msgId = headers.TryGetValue("cap-msg-id", out var id) ? id : "-";
        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["CapMessageId"] = msgId,
            ["Topic"] = nameof(TrustYieldOperationAppliedIntegrationEvent),
            ["TrustId"] = message.TrustId,
            ["PortfolioId"] = message.PortfolioId,
            ["ClosingDate"] = message.ClosingDate.ToString("O")
        }))
        {
            try
            {
                var result = await mediator.Send(new UpdateTrustFromYieldCommand(
                    message.TrustId, message.PortfolioId, message.ClosingDate,
                    message.YieldAmount, message.YieldRetention, message.ClosingBalance
                     ), cancellationToken); 

                if (result.IsFailure)
                {
                    logger.LogWarning("Regla de negocio: {Code} - {Desc}", result.Error.Code, result.Error.Description);
                    return; 
                }

            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("CAP Cancelado antes o durante mediator.Send");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CAP Excepción alrededor de mediator.Send");
                throw;
            }
        }
    }
}