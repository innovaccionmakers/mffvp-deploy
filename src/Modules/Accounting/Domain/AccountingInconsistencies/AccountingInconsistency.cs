using Common.SharedKernel.Core.Primitives;

namespace Accounting.Domain.AccountingInconsistencies;

public class AccountingInconsistency
{
    public long PortfolioId {get; private set;}
    public string Transaction {get; private set;}
    public string Inconsistency {get; private set;}
    public string? Activity {get; private set;} = string.Empty;

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
