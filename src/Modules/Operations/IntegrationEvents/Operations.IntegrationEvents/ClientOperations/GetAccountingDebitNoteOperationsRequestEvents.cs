namespace Operations.IntegrationEvents.ClientOperations;

public sealed record class GetAccountingDebitNoteOperationsRequestEvents(
    IEnumerable<int> PortfolioId,
    DateTime ProcessDate
);
