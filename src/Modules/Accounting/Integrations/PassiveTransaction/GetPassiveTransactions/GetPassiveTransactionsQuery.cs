using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransactions
{
    [AuditLog]
    public class GetPassiveTransactionsQuery() : IQuery<IReadOnlyCollection<GetPassiveTransactionsResponse>>;
}
