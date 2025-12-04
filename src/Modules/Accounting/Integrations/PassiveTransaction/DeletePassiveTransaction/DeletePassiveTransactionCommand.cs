using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.DeletePassiveTransaction
{
    [AuditLog]
    public sealed record class DeletePassiveTransactionCommand(
        int PortfolioId,
        long TypeOperationsId
        ) : ICommand;
}
