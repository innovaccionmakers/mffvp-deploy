using Closing.Application.PreClosing.Services.ExtraReturns.Dto;

using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Operations.TrustOperations;

public interface ITrustOperationsLocator
{
    Task<Result<IReadOnlyCollection<TrustOperationRemoteResponse>>> GetTrustOperationsAsync(
        int portfolioId,
        DateTime processDateUtc,
        CancellationToken cancellationToken);
}
