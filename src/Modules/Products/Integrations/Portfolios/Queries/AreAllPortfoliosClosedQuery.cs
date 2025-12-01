using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record AreAllPortfoliosClosedQuery(
    IEnumerable<int> PortfolioIds,
    DateTime Date
) : IQuery<bool>;

