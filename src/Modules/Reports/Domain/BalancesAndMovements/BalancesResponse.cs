namespace Reports.Domain.BalancesAndMovements
{
    public sealed record BalancesResponse(
        string? StartDate,
        string? EndDate,
        string? IdentificationType,
        string? Identification,
        string? FullName,
        string? TargetID,
        string? Target,
        string? Fund,
        string? Plan,
        string? Alternative,
        string? Portfolio,
        string? InitialBalance,
        string? Entry,
        string? Outflows,
        string? Returns,
        string? SourceWithholding,
        string? ClosingBalance
        );
}
