using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Services.External;
using Trusts.IntegrationEvents.ObjectiveTrustValidation;

namespace Products.Infrastructure.External.ObjectivesValidation;

internal sealed class ObjectivesValidationTrusts(IRpcClient rpc) : IObjectivesValidationTrusts
{
    public async Task<Result<ObjectivesValidationData>> ValidateAsync(
        int ObjectiveId,
        string RequestedStatus,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            ValidateObjectiveTrustRequest,
            ValidateObjectiveTrustResponse>(
            new ValidateObjectiveTrustRequest(ObjectiveId, RequestedStatus),
            ct);

        return new ObjectivesValidationData(
            rsp.CanUpdate,
            rsp.HasTrust,
            rsp.HasTrustWithBalance,
            rsp.Code,
            rsp.Message
        );
    }

}
