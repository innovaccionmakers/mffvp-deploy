namespace Operations.Integrations.Contributions;

public sealed record ContributionResponse(
    long? OperationId,
    string? PortfolioId,
    string? PortfolioName,
    string? TaxCondition,
    decimal? ContingentWithholdingValue
);