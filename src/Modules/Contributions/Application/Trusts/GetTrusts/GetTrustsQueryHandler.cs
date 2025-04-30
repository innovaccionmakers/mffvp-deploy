using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.Trusts;
using Contributions.Integrations.Trusts.GetTrusts;
using Contributions.Integrations.Trusts;
using System.Collections.Generic;
using System.Linq;

namespace Contributions.Application.Trusts.GetTrusts;

internal sealed class GetTrustsQueryHandler(
    ITrustRepository trustRepository)
    : IQueryHandler<GetTrustsQuery, IReadOnlyCollection<TrustResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustResponse>>> Handle(GetTrustsQuery request, CancellationToken cancellationToken)
    {
        var entities = await trustRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new TrustResponse(
                e.TrustId,
                e.AffiliateId,
                e.ObjectiveId,
                e.PortfolioId,
                e.TotalBalance,
                e.TotalUnits,
                e.Principal,
                e.Earnings,
                e.TaxCondition,
                e.ContingentWithholding,
                e.EarningsWithholding,
                e.AvailableBalance))
            .ToList();

        return Result.Success<IReadOnlyCollection<TrustResponse>>(response);
    }
}