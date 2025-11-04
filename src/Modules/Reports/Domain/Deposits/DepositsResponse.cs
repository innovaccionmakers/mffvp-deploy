namespace Reports.Domain.Deposits
{
    public record class DepositsResponse
    (
        string AccountType,
        string AccountNumber,
        string TransactionCode,
        DateTime EffectiveDate,
        decimal TransactionValue,
        string CheckNumber,
        string Nature,
        string Observations,
        string TransactionName,
        string AdditionalInfo,
        string Reference1,
        string Reference2,
        string Reference3,
        int Branch
    );
}
