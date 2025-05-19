using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.UpdatePortfolio;

public sealed record UpdatePortfolioCommand(
    long PortfolioId,
    string NewStandardCode,
    string NewName,
    string NewShortName,
    int NewModalityId,
    decimal NewInitialMinimumAmount
) : ICommand<PortfolioResponse>;