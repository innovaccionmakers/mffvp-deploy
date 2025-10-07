
namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed record ProcessPendingTransactionsResponse(
    bool Succeeded,
    string Status,      // "Processed" | "NothingToProcess" | "Error"
    int AppliedCount,
    int SkippedCount,
    string Code,
    string? Message = null
);
