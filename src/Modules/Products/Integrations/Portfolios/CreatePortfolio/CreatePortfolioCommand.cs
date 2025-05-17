using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.CreatePortfolio;

public sealed record CreatePortfolioCommand(
    string StandardCode,
    string Name,
    string ShortName,
    int ModalityId,
    decimal InitialMinimumAmount
) : ICommand<PortfolioResponse>;