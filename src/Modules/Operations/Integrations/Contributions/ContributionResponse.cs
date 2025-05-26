namespace Operations.Integrations.Contributions;

public sealed record ContributionResponse(
    string  Status,
    int     ResponseCode,
    string  ResponseDescription,
    long?   OperationId,
    string? PortfolioId,
    string? PortfolioName,
    string? TaxCondition,
    decimal? ContingentWithholdingValue
);