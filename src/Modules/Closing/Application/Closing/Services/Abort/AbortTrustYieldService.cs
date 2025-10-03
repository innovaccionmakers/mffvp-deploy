
using Closing.Domain.TrustYields;

namespace Closing.Application.Closing.Services.Abort;

public sealed class AbortTrustYieldService : IAbortTrustYieldService
{
    private readonly ITrustYieldRepository trustYieldRepository;

    public AbortTrustYieldService(
        ITrustYieldRepository trustYieldRepository)
    {
        this.trustYieldRepository = trustYieldRepository;
    }

    public async Task<int> DeleteTrustYieldsAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var deleted = await trustYieldRepository.DeleteByPortfolioAndDateAsync(
            portfolioId,
            closingDateUtc,
            cancellationToken);

        return deleted;
    }
}