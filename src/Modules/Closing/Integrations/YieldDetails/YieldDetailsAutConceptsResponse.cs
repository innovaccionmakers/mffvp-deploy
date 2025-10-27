using System.Text.Json;

namespace Closing.Integrations.YieldDetails
{
    public sealed record class YieldDetailsAutConceptsResponse(
        int PortfolioId,
        decimal Income,
        decimal Expenses
        );
}
