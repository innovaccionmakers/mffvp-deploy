using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Treasury.Integrations.BankAccounts.Response;

namespace Treasury.Integrations.BankAccounts.Commands;

[AuditLog]
public sealed record CreateBankAccountCommand(
    int PortfolioId,
    int IssuerId,
    string Issuer,
    string AccountNumber,
    string AccountType,
    string? Observations = null
) : ICommand<BankAccountResponse>;
