using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.GetPortfolio;

public sealed record GetPortfolioQuery(
    long PortfolioId
) : IQuery<PortfolioResponse>;