using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts;
using Trusts.Integrations.Trusts.GetTrusts;

namespace Trusts.Application.Trusts.GetTrusts;

internal sealed class GetTrustsQueryHandler(
    ITrustRepository trustRepository)
    : IQueryHandler<GetTrustsQuery, IReadOnlyCollection<TrustResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustResponse>>> Handle(GetTrustsQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await trustRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new TrustResponse(
                e.TrustId,
                e.AffiliateId,
                e.ClientId,
                e.CreationDate,
                e.ObjectiveId,
                e.PortfolioId,
                e.TotalBalance,
                e.TotalUnits,
                e.Principal,
                e.Earnings,
                e.TaxCondition,
                e.ContingentWithholding,
                e.EarningsWithholding,
                e.AvailableAmount,
                e.ContingentWithholdingPercentage))
            .ToList();

        return Result.Success<IReadOnlyCollection<TrustResponse>>(response);
    }
}