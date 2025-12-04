using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.UpdateTreasury
{
    [AuditLog]
    public sealed record class UpdateTreasuryCommand(
        int PortfolioId,
        string BankAccount,
        string? DebitAccount,
        string? CreditAccount
        ) : ICommand;
}
