using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using DataSync.Application.TrustSync;
using Trusts.IntegrationEvents.DataSync.TrustSync;

namespace DataSync.Infrastructure.External.Trusts;

internal sealed class TrustDataService(IRpcClient rpc) : ITrustDataService
{
    public async Task<Result<IReadOnlyCollection<TrustSyncData>>> GetActiveTrustsByPortfolioAsync(
        int portfolioId, 
        CancellationToken cancellationToken)
    {
        var response = await rpc.CallAsync<ActiveTrustsByPortfolioRequest, ActiveTrustsByPortfolioResponse>(
            new ActiveTrustsByPortfolioRequest(portfolioId),
            cancellationToken);

        if (!response.Success)
            return Result.Failure<IReadOnlyCollection<TrustSyncData>>(
                Error.Validation(response.ErrorCode!, response.ErrorMessage!));

        var trustSyncData = response.Trusts.Select(t => new TrustSyncData(
            (int)t.TrustId,
            t.PortfolioId,
            t.TotalBalance,
            t.Principal,
            t.ContingentWithholding)).ToList();

        return Result.Success<IReadOnlyCollection<TrustSyncData>>(trustSyncData);
    }
}
