
namespace Products.IntegrationEvents.AccumulatedCommissions;

public sealed record UpdateAccumulatedCommissionFromClosingResponse(bool Succeeded, string Status, string Code, string? Message);