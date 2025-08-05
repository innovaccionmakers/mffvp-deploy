namespace Treasury.Domain.TreasuryMovements;

public sealed record TreasuryMovementConcept(
    long TreasuryConceptId,
    decimal Value,
    long BankAccountId,
    long EntityId,
    long CounterpartyId
);
