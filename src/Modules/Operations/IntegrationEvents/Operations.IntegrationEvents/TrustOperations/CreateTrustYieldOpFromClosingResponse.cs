
namespace Operations.IntegrationEvents.TrustOperations;

public sealed record CreateTrustYieldOpFromClosingResponse(
       bool Succeeded,
       string? Code,
       string? Message,
       int Inserted,
       int Updated,
       IReadOnlyCollection<long> ChangedTrustIds
   );