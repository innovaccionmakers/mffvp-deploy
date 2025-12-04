using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.DeleteTreasury
{
    [AuditLog]
    public sealed record class DeleteTreasuryCommand(
        int PortfolioId,
        string BankAccount
        ) : ICommand;
}
