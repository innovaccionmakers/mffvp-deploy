using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransactions
{
    [AuditLog]
    public sealed record class GetPassiveTransactionsQuery(
        int PortfolioId,
        long TypeOperationsId
        ) : IQuery<GetPassiveTransactionsResponse>;
}
