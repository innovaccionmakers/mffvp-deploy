using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransaction
{
    [AuditLog]
    public sealed record class GetPassiveTransactionQuery(
        int PortfolioId,
        long TypeOperationsId
        ) : IQuery<GetPassiveTransactionResponse>;
}
