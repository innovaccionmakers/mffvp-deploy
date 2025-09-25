using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingFees;

public sealed record GetAccountingFeesCommand(
    List<int> PortfolioIds,
    DateTime ProcessDate
) : ICommand<bool>;
