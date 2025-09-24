using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingFees;

public sealed record GetAccountingFeesQuery(
    List<int> PortfolioIds,
    DateTime ProcessDate
) : IQuery<bool>;
