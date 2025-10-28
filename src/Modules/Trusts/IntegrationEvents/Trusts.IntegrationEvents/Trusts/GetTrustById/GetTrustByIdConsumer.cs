using Common.SharedKernel.Application.Rpc;
using MediatR;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.IntegrationEvents.Trusts.GetTrustById;

public sealed class GetTrustByIdConsumer(ISender sender)
    : IRpcHandler<GetTrustByIdRequest, GetTrustByIdResponse>
{
    public async Task<GetTrustByIdResponse> HandleAsync(
        GetTrustByIdRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTrustByIdQuery(request.TrustId), cancellationToken);

        if (!result.IsSuccess)
        {
            return new GetTrustByIdResponse(false, result.Error.Code, result.Error.Description, null);
        }

        var trustDetails = result.Value is null
            ? null
            : new TrustDetails(
                result.Value.TrustId,
                result.Value.AffiliateId,
                result.Value.ClientOperationId,
                result.Value.CreationDate,
                result.Value.ObjectiveId,
                result.Value.PortfolioId,
                result.Value.TotalBalance,
                result.Value.TotalUnits,
                result.Value.Principal,
                result.Value.Earnings,
                result.Value.TaxCondition,
                result.Value.ContingentWithholding,
                result.Value.EarningsWithholding,
                result.Value.AvailableAmount,
                result.Value.Status,
                result.Value.UpdateDate);

        return new GetTrustByIdResponse(true, null, null, trustDetails);
    }
}
