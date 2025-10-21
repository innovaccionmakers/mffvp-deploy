using Closing.Integrations.Yields.Queries;

namespace Closing.IntegrationEvents.Yields
{
    public sealed record class GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse(
        bool IsValid,
        string? Code,
        string? Message,
        IReadOnlyCollection<YieldAutConceptsResponse> Yields);
}