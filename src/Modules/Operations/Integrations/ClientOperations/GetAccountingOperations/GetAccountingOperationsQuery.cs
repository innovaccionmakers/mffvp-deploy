using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetAccountingOperations
{
    public sealed record class GetAccountingOperationsQuery(IEnumerable<int> PortfolioId, DateTime ProcessDate, string OperationTypeName, string ClientOperationTypeName) : IQuery<IReadOnlyCollection<GetAccountingOperationsResponse>>;
}
