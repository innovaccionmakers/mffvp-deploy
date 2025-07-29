
using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Trusts.Trusts;

public interface ITrustLocator
{
    Task<Result<IReadOnlyCollection<ActiveTrustsByPortfolioRemoteResponse>>> GetActiveTrustsAsync(
        int portfolioId,
        CancellationToken cancellationToken);
}

public sealed record ActiveTrustsByPortfolioRemoteResponse(
    long TrustId,
    int PortfolioId,
    decimal TotalBalance,
    decimal Principal,
    decimal ContingentWithholding);