using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.GetBalances;

namespace Trusts.Application.Balances.GetBalances;

internal sealed class GetBalancesQueryHandler(
    ITrustRepository repository)
    : IQueryHandler<GetBalancesQuery, IReadOnlyCollection<BalanceResponse>>
{
    public async Task<Result<IReadOnlyCollection<BalanceResponse>>> Handle(
        GetBalancesQuery request,
        CancellationToken cancellationToken)
    {
        var balances = await repository.GetBalancesAsync(request.AffiliateId, cancellationToken);
        var response = balances
            .Select(b => new BalanceResponse(
                b.ObjectiveId,
                b.PortfolioId,
                b.TotalBalance,
                b.AvailableAmount,
                b.ProtectedBalance,
                b.AgileWithdrawalAvailable))
            .ToList();
        return Result.Success<IReadOnlyCollection<BalanceResponse>>(response);
    }
}
