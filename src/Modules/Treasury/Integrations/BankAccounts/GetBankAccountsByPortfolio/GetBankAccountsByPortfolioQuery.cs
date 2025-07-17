using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.BankAccounts;

namespace Treasury.Integrations.BankAccounts.GetBankAccountsByPortfolio;

public record GetBankAccountsByPortfolioQuery(long PortfolioId) : IQuery<IReadOnlyCollection<BankAccount>>;