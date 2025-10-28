using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.Application.Queries;

internal sealed class GetTrustByIdQueryHandler(ITrustRepository trustRepository)
    : IQueryHandler<GetTrustByIdQuery, TrustDetailsResult?>
{
    public async Task<Result<TrustDetailsResult?>> Handle(GetTrustByIdQuery request, CancellationToken cancellationToken)
    {
        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);

        if (trust is null)
        {
            return Result.Success<TrustDetailsResult?>(null);
        }

        var trustDetails = new TrustDetailsResult(
            trust.TrustId,
            trust.AffiliateId,
            trust.ClientOperationId,
            trust.CreationDate,
            trust.ObjectiveId,
            trust.PortfolioId,
            trust.TotalBalance,
            trust.TotalUnits,
            trust.Principal,
            trust.Earnings,
            trust.TaxCondition,
            trust.ContingentWithholding,
            trust.EarningsWithholding,
            trust.AvailableAmount,
            trust.Status,
            trust.UpdateDate);

        return Result.Success<TrustDetailsResult?>(trustDetails);
    }
}
