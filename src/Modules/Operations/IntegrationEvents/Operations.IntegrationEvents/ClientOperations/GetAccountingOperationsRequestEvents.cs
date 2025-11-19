namespace Operations.IntegrationEvents.ClientOperations
{
    public sealed record class GetAccountingOperationsRequestEvents(
        IEnumerable<int> PortfolioId,
        DateTime ProcessDate,
        string OperationTypeName,
        string ClientOperationTypeName
    );
}
