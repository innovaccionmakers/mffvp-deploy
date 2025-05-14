using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Portfolios.GetPortfolio;
public sealed record GetPortfolioQuery(
    long PortfolioId
) : IQuery<PortfolioResponse>;