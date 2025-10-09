

namespace Products.IntegrationEvents.Portfolio.UpdateFromClosing;

public sealed record UpdatePortfolioFromClosingResponse(
    bool Succeeded,
    string Status,      // "Updated" | "NoChange" | "Error"
    int UpdatedCount,
    string Code,
    string? Message = null
);
