namespace Operations.Domain.ClientOperations
{
    public sealed record class AccountingOperationsResponse(
        int PortfolioId,
        int AffiliateId,
        decimal Amount,
        long OperationTypeId,
        string CollectionAccount
    );
}
