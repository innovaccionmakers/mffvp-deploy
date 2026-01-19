namespace Operations.Domain.ClientOperations
{
    public sealed record class AccountingOperationsResponse(
        long ClientOperationId,
        int PortfolioId,
        int AffiliateId,
        decimal Amount,
        long OperationTypeId,
        string CollectionAccount,
        long? LinkedClientOperationId = null
    );
}
