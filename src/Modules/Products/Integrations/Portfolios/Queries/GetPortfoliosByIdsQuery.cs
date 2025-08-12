using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record GetPortfoliosByIdsQuery(IEnumerable<long> PortfolioIds) : IQuery<IReadOnlyCollection<PortfolioResponse>>;
