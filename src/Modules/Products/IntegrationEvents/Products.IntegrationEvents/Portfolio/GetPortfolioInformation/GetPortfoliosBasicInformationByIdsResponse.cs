namespace Products.IntegrationEvents.Portfolio.GetPortfolioInformation;

public sealed record PortfolioBasicInformationResponse(
    int PortfolioId,
    string NitApprovedPortfolio,
    int VerificationDigit,
    string Name
);

public sealed record GetPortfoliosBasicInformationByIdsResponse(
    bool Succeeded,
    IReadOnlyCollection<PortfolioBasicInformationResponse> Portfolios,
    string? Code,
    string? Message
);

