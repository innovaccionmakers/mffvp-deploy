using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Portfolios.DeletePortfolio;
public sealed record DeletePortfolioCommand(
    long PortfolioId
) : ICommand;