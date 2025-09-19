using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetAccountingOperations
{
    public sealed record class GetAccountingOperationsQuery(List<int> PortfolioId, DateTime ProcessDate) : IQuery<IReadOnlyCollection<GetAccountingOperationsResponse>>;
}
