namespace Reports.Domain.BalancesAndMovements
{
    public sealed record MovementsResponse(
        string Date,
        string IdentificationType,
        string Identification,
        string AffiliateName,
        string TargetID,
        string Target,
        string FundName,
        string Plan,
        string Alternative,
        string Portfolio,
        string Receipt,
        string TransactionType,
        string TransactionSubtype,
        string Value,
        string TaxCondition,
        string ContingentWithholdingDue,
        string PaymentMethod
        );
}
