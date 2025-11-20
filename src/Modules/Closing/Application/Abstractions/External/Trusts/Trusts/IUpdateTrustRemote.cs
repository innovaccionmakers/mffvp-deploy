using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Trusts.Trusts;


public sealed record UpdateTrustFromYieldItem(
    long TrustId,
    decimal YieldAmount,
    decimal YieldRetentionRate,
    decimal ClosingBalance
);

public sealed record UpdateTrustFromYieldBulkRemoteRequest(
    int PortfolioId,
    DateTime ClosingDate,
    int BatchIndex,
    IReadOnlyList<UpdateTrustFromYieldItem> TrustsToUpdate,
    string? IdempotencyKey = null
);

public sealed record UpdateTrustFromYieldBulkRemoteResponse(
    int BatchIndex,
    int Updated,
    IReadOnlyCollection<long> MissingTrustIds,
    IReadOnlyCollection<long> ValidationMismatchTrustIds
);

/// <summary>
/// Command remoto al dominio Trusts para aplicar rendimiento al saldo del fideicomiso.
/// </summary>
public interface IUpdateTrustRemote
{
    Task<Result<UpdateTrustFromYieldBulkRemoteResponse>> UpdateFromYieldAsync(
        UpdateTrustFromYieldBulkRemoteRequest request,
        CancellationToken cancellationToken);
}