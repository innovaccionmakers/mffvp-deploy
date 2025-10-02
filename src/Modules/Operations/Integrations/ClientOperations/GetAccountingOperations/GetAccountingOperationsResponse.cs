using Common.SharedKernel.Domain.OperationTypes;

namespace Operations.Integrations.ClientOperations.GetAccountingOperations
{
    public sealed record class GetAccountingOperationsResponse
    (
        int PortfolioId,
        int AffiliateId,
        decimal Amount,
        string OperationTypeName,
        IncomeEgressNature Nature,
        long OperationTypeId,
        string CollectionAccount
    );
}
