namespace Products.Integrations.Portfolios;

public sealed record PortfolioResponse(
    long PortfolioId,
    string StandardCode,
    string Name,
    string ShortName,
    int ModalityId,
    decimal InitialMinimumAmount
);