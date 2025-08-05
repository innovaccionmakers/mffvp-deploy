using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryConcepts.Response;

namespace Treasury.Integrations.TreasuryMovements.Commands;

public sealed record CreateTreasuryMovementCommand(
    int PortfolioId,
    DateTime ClosingDate,
    IReadOnlyCollection<TreasuryMovementConcept> Concepts
) : ICommand<TreasuryMovementResponse>;