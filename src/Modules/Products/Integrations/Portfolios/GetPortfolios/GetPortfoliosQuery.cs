using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.GetPortfolios;

public sealed record GetPortfoliosQuery : IQuery<IReadOnlyCollection<PortfolioResponse>>;