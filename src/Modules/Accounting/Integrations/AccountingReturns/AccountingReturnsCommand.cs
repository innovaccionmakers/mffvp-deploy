using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingReturns;

public sealed record AccountingReturnsCommand(
    IEnumerable<int> PortfolioIds,
    DateTime ProcessDate
 ) : ICommand<bool>;
