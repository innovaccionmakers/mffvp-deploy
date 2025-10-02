using System.Text.Json.Serialization;

namespace Accounting.Domain.AccountingInconsistencies;

public class AccountingInconsistency
{
    [JsonPropertyName("portfolioId")]
    public long PortfolioId {get; set;}

    [JsonPropertyName("transaction")]
    public string Transaction {get; set;} = string.Empty;

    [JsonPropertyName("inconsistency")]
    public string Inconsistency {get; set;} = string.Empty;

    [JsonPropertyName("activity")]
    public string? Activity {get; set;} = string.Empty;

    // Constructor público sin parámetros para serialización JSON
    public AccountingInconsistency()
    {
    }

    private AccountingInconsistency(
        long portfolioId,
        string transaction,
        string inconsistency,
        string? activity
        )
    {
        PortfolioId = portfolioId;
        Transaction = transaction;
        Activity = activity;
        Inconsistency = inconsistency;
    }

    public static AccountingInconsistency Create(
        long portfolioId,
        string transaction,
        string inconsistency,
        string? activity = "")
    {
        return new AccountingInconsistency(
            portfolioId,
            transaction,
            inconsistency,
            activity
        );
    }
}
