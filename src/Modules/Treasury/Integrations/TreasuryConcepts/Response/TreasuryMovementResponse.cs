namespace Treasury.Integrations.TreasuryConcepts.Response;

public sealed record TreasuryMovementResponse(
    IReadOnlyCollection<long> TreasuryMovementIds
);
