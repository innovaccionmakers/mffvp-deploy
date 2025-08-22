using Associate.IntegrationEvents.ActivateValidation;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.Application.Abstractions.Services.External;

namespace Products.Infrastructure.External.Affiliates;

internal sealed class AffiliateLocator(IRpcClient rpc) : IAffiliateLocator
{
    public async Task<Result<int?>> FindAsync(
        string docTypeCode,
        string identification,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            new GetActivateIdByIdentificationRequest(docTypeCode, identification),
            ct);

        if (!rsp.Succeeded)
            return Result.Failure<int?>(
                Error.Validation(rsp.Code, rsp.Message));

        return Result.Success<int?>(rsp.Activate?.ActivateId);
    }
}