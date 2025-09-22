namespace Operations.IntegrationEvents.ClientOperations
{
    public sealed record class GetAccountingOperationsRequestEvents(
        List<int> PortfolioId,
        DateTime ProcessDate
        );
}
