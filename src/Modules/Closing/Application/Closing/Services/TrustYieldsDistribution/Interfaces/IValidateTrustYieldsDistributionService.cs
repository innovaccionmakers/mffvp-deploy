
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces
{
    public interface IValidateTrustYieldsDistributionService
    {
        Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
    }
}
