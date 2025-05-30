using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Messaging;
using Products.Application.Abstractions.Services.External;

namespace Products.Infrastructure.External.Affiliates;

internal sealed class AffiliateLocator(ICapRpcClient rpc) : IAffiliateLocator
{
    public async Task<(bool Found, int? Id)> FindAsync(
        string docTypeCode, string identification, CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            nameof(GetActivateIdByIdentificationRequest),
            new(docTypeCode, identification),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.Succeeded
            ? (true, rsp.ActivateId)
            : (false, null);
    }
}