
namespace Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;

public sealed record AccountingConceptsRequestEvent(
    IEnumerable<int> PortfolioIds,
    DateTime ProcessDate
    );