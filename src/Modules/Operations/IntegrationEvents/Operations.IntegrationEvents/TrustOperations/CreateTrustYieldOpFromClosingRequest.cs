
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed record CreateTrustYieldOpFromClosingRequest(
        int PortfolioId,
    DateTime ClosingDateUtc,
    long OperationTypeId,
    IReadOnlyList<TrustYieldOperationFromClosing> TrustYieldOperations,
    string? IdempotencyKey = null
   );