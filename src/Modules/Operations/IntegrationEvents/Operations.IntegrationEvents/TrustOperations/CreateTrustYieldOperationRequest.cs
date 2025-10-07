

namespace Operations.IntegrationEvents.TrustOperations;

public sealed record CreateTrustYieldOperationRequest(
       long TrustId,
       int PortfolioId,
       DateTime ClosingDate,
       long OperationTypeId,
       decimal YieldAmount,
       decimal YieldRetention,
       DateTime ProcessDate,
       decimal ClosingBalance,
       string? IdempotencyKey = null
   );