using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingFees;

public sealed record AccountingFeesCommand(
    IEnumerable<int> PortfolioIds,
    DateTime ProcessDate
) : ICommand<bool>;
