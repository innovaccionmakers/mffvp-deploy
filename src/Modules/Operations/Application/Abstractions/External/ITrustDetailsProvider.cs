using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ITrustDetailsProvider
{
    Task<Result<TrustDetailsResult>> GetAsync(long trustId, CancellationToken cancellationToken);
}

public sealed record TrustDetailsResult(decimal Earnings);