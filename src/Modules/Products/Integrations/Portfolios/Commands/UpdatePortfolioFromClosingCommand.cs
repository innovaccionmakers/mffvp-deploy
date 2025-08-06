using MediatR;

namespace Products.Integrations.Portfolios.Commands;

public sealed record UpdatePortfolioFromClosingCommand(
    int PortfolioId,
    DateTime CloseDate
) : IRequest;