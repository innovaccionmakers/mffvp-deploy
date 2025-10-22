namespace Operations.Integrations.TrustOperations.Commands;

public sealed record UpsertTrustOpFromClosingResponse(
    int Inserted,
    int Updated,
    IReadOnlyCollection<long> ChangedTrustIds // insertados o actualizados
  );