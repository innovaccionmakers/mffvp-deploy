namespace Reports.Domain.BalancesAndMovements
{
    public sealed record BalancesResponse(
        string StartDate,
        string EndDate,
        string IdentificationType,
        string Identification,
        string AffiliateName,
        string TargetID,
        string Target,
        string FundName,
        string Plan,
        string Alternative,
        string Portfolio,
        string StartingBalance,
        string Inflows,
        string Outflows,
        string Returns,
        string SourceWithholding,
        string ClosingBalance
        );
}
