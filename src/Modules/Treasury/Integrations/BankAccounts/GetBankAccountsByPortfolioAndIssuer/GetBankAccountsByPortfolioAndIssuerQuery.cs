using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.BankAccounts;

namespace Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolioAndIssuer;

public record GetBankAccountsByPortfolioAndIssuerQuery(
    long PortfolioId,
    long IssuerId
) : IQuery<IReadOnlyCollection<BankAccount>>;