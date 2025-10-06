
namespace Closing.Application.Closing.Services.Abort;

public interface IAbortTrustYieldService
{
    Task<int> DeleteTrustYieldsAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken);
}
