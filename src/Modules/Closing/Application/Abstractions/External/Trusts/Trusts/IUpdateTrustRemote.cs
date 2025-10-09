using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Trusts.Trusts;

public sealed record UpdateTrustFromYieldRemoteRequest(
    int PortfolioId,
    DateTime ClosingDate,
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

/// <summary>
/// Command remoto al dominio Trusts para aplicar rendimiento al saldo del fideicomiso.
/// </summary>
public interface IUpdateTrustRemote
{
    Task<Result<UpdateTrustFromYieldRemoteResponse>> UpdateFromYieldAsync(
        UpdateTrustFromYieldRemoteRequest request,
        CancellationToken cancellationToken);
}