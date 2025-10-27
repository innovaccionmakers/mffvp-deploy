using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Trusts.Integrations.Trusts.PutTrust;

namespace Trusts.IntegrationEvents.Trusts.PutTrust;

public sealed class PutTrustConsumer : IRpcHandler<PutTrustRequest, PutTrustResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<PutTrustConsumer> logger;

    public PutTrustConsumer(ISender mediator, ILogger<PutTrustConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<PutTrustResponse> HandleAsync(PutTrustRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new PutTrustCommand(
                request.ClientOperationId,
                request.Status,
                request.TotalBalance,
                request.TotalUnits,
                request.Principal,
                request.Earnings,
                request.ContingentWithholding,
                request.EarningsWithholding,
                request.AvailableAmount,
                request.UpdateDate);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return new PutTrustResponse(false, result.Error.Code, result.Error.Description);
            }

            return new PutTrustResponse(true, null, null);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{Handler} - Cancelado por token.", nameof(PutTrustConsumer));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Handler} - Excepción no controlada.", nameof(PutTrustConsumer));
            return new PutTrustResponse(false, "TRU-UNHANDLED", ex.Message);
        }
    }
}
