using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;

namespace Operations.Infrastructure.External.Activate;

internal sealed class ActivateLocator(ICapRpcClient rpc) : IActivateLocator
{
    public async Task<Result<(bool Found, int ActivateId, bool IsPensioner)>> FindAsync(
        string idType,
        string identification,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            nameof(GetActivateIdByIdentificationRequest),
            new GetActivateIdByIdentificationRequest(idType, identification),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.Succeeded
            ? Result.Success((true, rsp.Activate!.ActivateId, rsp.Activate.Pensioner))
            : Result.Failure<(bool, int, bool)>(Error.Validation(rsp.Code, rsp.Message));
    }
}