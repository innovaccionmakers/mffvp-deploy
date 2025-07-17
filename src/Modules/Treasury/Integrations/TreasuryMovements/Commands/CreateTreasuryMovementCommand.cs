using Common.SharedKernel.Application.Messaging;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Integrations.TreasuryMovements.Commands;

public sealed record CreateTreasuryMovementCommand(
    int PortfolioId,
    DateTime ClosingDate,
    long TreasuryConceptId,
    decimal Value,
    DateTime ProcessDate,
    long BankAccountId,
    long EntityId,
    long CounterpartyId
) : ICommand<TreasuryMovementResponse>;
