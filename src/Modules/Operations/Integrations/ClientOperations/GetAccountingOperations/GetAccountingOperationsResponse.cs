namespace Operations.Integrations.ClientOperations.GetAccountingOperations
{
    public sealed record class GetAccountingOperationsResponse
        (
        int PortfolioId,
        int AffiliateId,
        decimal Amount,
        long OperationTypeId
        );
}
