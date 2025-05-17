using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.DeletePortfolio;

public sealed record DeletePortfolioCommand(
    long PortfolioId
) : ICommand;