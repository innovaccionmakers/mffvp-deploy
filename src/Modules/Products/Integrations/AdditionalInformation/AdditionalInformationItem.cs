namespace Products.Integrations.AdditionalInformation;

public sealed record AdditionalInformationItem(
    int PortfolioId,
    string PortfolioName,
    int ObjectiveId,
    string ObjectiveName,
    int AlternativeId,
    string AlternativeName,
    int FundId,
    string FundName,
    string PortfolioCode,
    string ObjectiveCode,
    string AlternativeCode,
    string FundCode);
