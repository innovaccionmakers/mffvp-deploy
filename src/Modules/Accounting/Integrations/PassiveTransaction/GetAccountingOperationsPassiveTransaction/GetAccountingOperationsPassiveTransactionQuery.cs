using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction
{
    public sealed record class GetAccountingOperationsPassiveTransactionQuery(
        IEnumerable<int> PortfolioIds,
        IEnumerable<long> OperationTypeId
        ) : IQuery<IReadOnlyCollection<GetAccountingOperationsPassiveTransactionResponse>>;
}
