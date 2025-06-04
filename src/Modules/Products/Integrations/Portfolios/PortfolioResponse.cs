namespace Products.Integrations.Portfolios;

public sealed record PortfolioResponse(
    long PortfolioId,
    string HomologatedCode,
    string Name,
    string ShortName,
    int ModalityId,
    decimal InitialMinimumAmount
);