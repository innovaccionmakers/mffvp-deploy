namespace Contributions.Integrations.FullContribution
{
    public sealed record FullContributionResponse(
    Guid OperationId,
    string PortfolioCode,
    string PortfolioName,
    string TaxConditionDescription,
    decimal ContingentWithholdingValue
);
}
