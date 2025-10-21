using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ITrustInfoProvider
{
    Task<Result<TrustInfoResult>> GetAsync(long clientOperationId, decimal contributionValue, CancellationToken cancellationToken);
}

public sealed record TrustInfoResult(long TrustId);
