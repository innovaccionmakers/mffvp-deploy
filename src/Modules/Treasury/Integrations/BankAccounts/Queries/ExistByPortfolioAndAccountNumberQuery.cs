using Common.SharedKernel.Application.Messaging;

namespace Treasury.Integrations.BankAccounts.Queries;

public record ExistByPortfolioAndAccountNumberQuery(long PortfolioId, string AccountNumber) : IQuery<bool>;
