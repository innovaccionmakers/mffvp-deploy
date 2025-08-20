using Associate.IntegrationEvents.ActivateValidation;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.Application.Abstractions.External;

namespace Operations.Infrastructure.External.Activate;

internal sealed class ActivateLocator(IRpcClient rpc) : IActivateLocator
{
    public async Task<Result<(bool Found, int ActivateId, bool IsPensioner)>> FindAsync(
        string idType,
        string identification,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            new GetActivateIdByIdentificationRequest(idType, identification),
            ct);

        return rsp.Succeeded
            ? Result.Success((true, rsp.Activate!.ActivateId, rsp.Activate.Pensioner))
            : Result.Failure<(bool, int, bool)>(Error.Validation(rsp.Code, rsp.Message));
    }
}