using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.PassiveTransaction.DeletePassiveTransaction
{
    public sealed record class DeletePassiveTransactionCommand(
        int PortfolioId,
        long TypeOperationsId
        ) : ICommand;
}
