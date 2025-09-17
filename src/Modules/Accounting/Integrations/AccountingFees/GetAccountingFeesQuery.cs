using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingFees;

public sealed record GetAccountingFeesQuery(
    List<int> PortfolioIds,
    DateTime ClosingDate
) : IQuery<bool>;
