
namespace Operations.IntegrationEvents.TrustOperations;

public sealed record CreateTrustYieldOperationResponse(
       bool Succeeded,
       string? Code,
       string? Message,
       long? OperationId = null
   );