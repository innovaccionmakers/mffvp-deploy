using Closing.Application.Abstractions.External.Trusts.Trusts;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Trusts.IntegrationEvents.DataSync.TrustSync;

namespace Closing.Infrastructure.External.Trusts.Trusts;

internal sealed class TrustLocator(IRpcClient rpcClient) : ITrustLocator
{
    public async Task<Result<IReadOnlyCollection<ActiveTrustsByPortfolioRemoteResponse>>> GetActiveTrustsAsync(
        int portfolioId,
        CancellationToken cancellationToken)
    {
        var request = new ActiveTrustsByPortfolioRequest(portfolioId);

        var response = await rpcClient.CallAsync<
            ActiveTrustsByPortfolioRequest,
            ActiveTrustsByPortfolioResponse>(
            request,
            cancellationToken
        );

        IReadOnlyCollection<ActiveTrustsByPortfolioRemoteResponse>? trusts = response?.Trusts?
            .Select(t => new ActiveTrustsByPortfolioRemoteResponse(
                t.TrustId,
                t.PortfolioId,
                t.TotalBalance,
                t.Principal,
                t.ContingentWithholding))
            .ToList();

        return response.Success
            ? Result.Success(trusts!)
            : Result.Failure<IReadOnlyCollection<ActiveTrustsByPortfolioRemoteResponse>>(
                Error.Validation(response.ErrorCode!, response.ErrorMessage!));
    }
}

