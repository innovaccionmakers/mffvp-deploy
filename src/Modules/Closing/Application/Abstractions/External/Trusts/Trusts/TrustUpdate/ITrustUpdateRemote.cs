
using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Trusts.Trusts.TrustUpdate;

public sealed record UpdateTrustFromYieldRemoteRequest(
    long TrustId,
    decimal YieldAmount,
    decimal YieldRetention,
    decimal ClosingBalance,
    bool ValidateConsistency = true,
    bool Strict = false,
    decimal? Tolerance = null,
    string? CorrelationId = null
);

public sealed record UpdateTrustFromYieldRemoteResponse;

public interface ITrustUpdateRemote
{
    Task<Result<UpdateTrustFromYieldRemoteResponse>> UpdateFromYieldAsync(
        UpdateTrustFromYieldRemoteRequest request,
        CancellationToken cancellationToken);
}
