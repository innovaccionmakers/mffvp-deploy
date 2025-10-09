

namespace Operations.Integrations.TrustOperations.Commands;

public sealed record UpsertTrustOperationResponse(
      long? OperationId,
      bool Changed
  );