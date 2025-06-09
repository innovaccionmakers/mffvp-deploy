using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Trusts.IntegrationEvents.CreateTrust;

namespace Operations.Infrastructure.External.Trusts;

internal sealed class TrustCreator(ICapRpcClient rpc) : ITrustCreator
{
    public async Task<Result> CreateAsync(TrustCreationDto dto, CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            CreateTrustRequest,
            CreateTrustResponse>(
            nameof(CreateTrustRequest),
            new CreateTrustRequest(
                dto.AffiliateId,
                dto.ClientOperationId,
                dto.CreationDate,
                dto.ObjectiveId,
                dto.PortfolioId,
                dto.TotalBalance,
                dto.TotalUnits,
                dto.Principal,
                dto.Earnings,
                dto.TaxCondition,
                dto.ContingentWithholding,
                dto.EarningsWithholding,
                dto.AvailableAmount),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(rsp.Code ?? string.Empty, rsp.Message ?? string.Empty));
    }
}