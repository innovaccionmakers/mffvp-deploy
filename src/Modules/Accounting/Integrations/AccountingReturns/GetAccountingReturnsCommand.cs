using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingReturns;

public sealed record GetAccountingReturnsCommand(
    List<int> PortfolioIds,
    DateTime ProcessDate
 ) : ICommand<bool>;
