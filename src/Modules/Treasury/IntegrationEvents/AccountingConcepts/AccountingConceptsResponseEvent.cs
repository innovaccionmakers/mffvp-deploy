using Treasury.Domain.TreasuryMovements;

namespace Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;

public sealed record AccountingConceptsResponseEvent(
    bool IsValid,
    string? Code,
    string? Message,
    IReadOnlyCollection<TreasuryMovement> movements
);