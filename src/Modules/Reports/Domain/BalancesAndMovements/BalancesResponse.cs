namespace Reports.Domain.BalancesAndMovements
{
    public sealed record BalancesResponse(
        string StartDate,
        string EndDate,
        string IdentificationType,
        string Identification,
        string FullName,
        int ObjectiveId,
        string Objective,
        string Fund,
        string Plan,
        string Alternative,
        string Portfolio,
        decimal InitialBalance,
        decimal Entry,
        decimal Outflows,
        decimal Returns,
        decimal SourceWithholding,
        decimal ClosingBalance
        );
}
