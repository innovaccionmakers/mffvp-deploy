namespace Reports.Domain.BalancesAndMovements
{
    public sealed record MovementsResponse(
        string ProcesDate,
        string IdentificationType,
        string Identification,
        string FullName,
        int ObjectiveId,
        string Objective,
        string Fund,
        string Plan,
        string Alternative,
        string Portfolio,
        long Voucher,
        string TransactionType,
        string TransactionSubtype,
        decimal Value,
        string TaxCondition,
        decimal ContingentWithholding,
        string PaymentMethod,
        string CommercialUser,
        string Hour,
        string OriginAccount
        );
}
