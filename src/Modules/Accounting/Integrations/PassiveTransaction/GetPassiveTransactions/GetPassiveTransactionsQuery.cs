using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransactions
{
    public sealed record class GetPassiveTransactionsQuery(
        int PortfolioId,
        long TypeOperationsId
        ) : IQuery<GetPassiveTransactionsResponse>;
}
