
namespace Products.IntegrationEvents.AccumulatedCommissions;

public sealed record UpdateAccumulatedCommissionFromClosingRequest(int PortfolioId, int CommissionId, decimal AccumulatedValue, DateTime ClosingDate, string IdempotencyKey, string Origin, string? ExecutionId = null);
