namespace Closing.Integrations.Yields.Queries
{
    public sealed record class YieldAutConceptsResponse(
        long YieldId,
        int PortfolioId,
        decimal YieldToCredit,
        decimal CreditedYields,
        decimal YieldToDistributedValue
    );
}
