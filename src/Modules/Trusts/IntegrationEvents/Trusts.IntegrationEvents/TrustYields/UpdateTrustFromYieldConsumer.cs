

using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.IntegrationEvents.TrustYields;

public sealed class UpdateTrustFromYieldConsumer
  : IRpcHandler<UpdateTrustFromYieldRequest, UpdateTrustFromYieldResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<UpdateTrustFromYieldConsumer> logger;

    public UpdateTrustFromYieldConsumer(
        ISender mediator,
        ILogger<UpdateTrustFromYieldConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<UpdateTrustFromYieldResponse> HandleAsync(
        UpdateTrustFromYieldRequest request,
        CancellationToken ct)
    {
       
        try
        {
            var command = new UpdateTrustFromYieldCommand(
                PortfolioId: request.PortfolioId,
                ClosingDate: request.ClosingDate,
                TrustId: request.TrustId,
                YieldAmount: request.YieldAmount,
                YieldRetention: request.YieldRetention,
                ClosingBalance: request.ClosingBalance
            );

            var result = await mediator.Send(command, ct);

            if (result.IsFailure)
            {
                return new UpdateTrustFromYieldResponse(
                    Succeeded: false,
                    Code: result.Error.Code,
                    Message: result.Error.Description
                );
            }
            return new UpdateTrustFromYieldResponse(true, null, null);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{Handler} - Cancelado por token.", nameof(UpdateTrustFromYieldConsumer));
            throw; 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Handler} - Excepción no controlada.", nameof(UpdateTrustFromYieldConsumer));
            return new UpdateTrustFromYieldResponse(false, "TRU-UNHANDLED", ex.Message);
        }
    }
}