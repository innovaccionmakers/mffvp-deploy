using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.Trusts;
using Contributions.Integrations.Trusts.GetTrust;
using Contributions.Integrations.Trusts;

namespace Contributions.Application.Trusts.GetTrust;

internal sealed class GetTrustQueryHandler(
    ITrustRepository trustRepository)
    : IQueryHandler<GetTrustQuery, TrustResponse>
{
    public async Task<Result<TrustResponse>> Handle(GetTrustQuery request, CancellationToken cancellationToken)
    {
        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);
        if (trust is null)
        {
            return Result.Failure<TrustResponse>(TrustErrors.NotFound(request.TrustId));
        }
        var response = new TrustResponse(
            trust.TrustId,
            trust.AffiliateId,
            trust.ObjectiveId,
            trust.PortfolioId,
            trust.TotalBalance,
            trust.TotalUnits,
            trust.Principal,
            trust.Earnings,
            trust.TaxCondition,
            trust.ContingentWithholding,
            trust.EarningsWithholding,
            trust.AvailableBalance
        );
        return response;
    }
}