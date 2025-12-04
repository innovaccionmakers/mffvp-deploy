using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.Treasuries.CreateTreasury
{
    [AuditLog]
    public sealed record class CreateTreasuryCommand(
        int PortfolioId,
        string BankAccount,
        string? DebitAccount,
        string? CreditAccount
        ) : ICommand;
}
