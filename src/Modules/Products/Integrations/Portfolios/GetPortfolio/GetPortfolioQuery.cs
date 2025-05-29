using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.GetPortfolio;

public sealed record GetPortfolioQuery(
    int PortfolioId
) : IQuery<PortfolioResponse>;