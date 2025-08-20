using Common.SharedKernel.Application.Messaging;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Integrations.TreasuryMovements.Commands;

public sealed record CreateTreasuryMovementCommand(
    int PortfolioId,
    DateTime ClosingDate,
    IReadOnlyCollection<TreasuryMovementConcept> Concepts
) : ICommand<TreasuryMovementResponse>;

public sealed record TreasuryMovementConcept(
    long TreasuryConceptId,
    decimal Value,
    long BankAccountId,
    long CounterpartyId
);
