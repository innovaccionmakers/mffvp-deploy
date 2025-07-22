

using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public interface IDistributeTrustYieldsService
{
    Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}