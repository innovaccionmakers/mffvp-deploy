using Common.SharedKernel.Domain;

namespace Accounting.Domain.PassiveTransactions;

public sealed class PassiveTransaction : Entity
{
    public long PassiveTransactionId { get; private set; }
    public int PortfolioId { get; private set; }
    public long TypeOperationsId { get; private set; }
    public string? DebitAccount { get; private set; }
    public string? CreditAccount { get; private set; }
    public string? ContraCreditAccount { get; private set; }
    public string? ContraDebitAccount { get; private set; }

    public PassiveTransaction()
    {
    }

    public static Result<PassiveTransaction> Create(
        int portfolioId,
        long typeOperationsId,
        string? debitAccount,
        string? creditAccount,
        string? contraCreditAccount,
        string? contraDebitAccount)
    {
        var passiveTransaction = new PassiveTransaction
        {
            PassiveTransactionId = default,
            TypeOperationsId = typeOperationsId,
            PortfolioId = portfolioId,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            ContraCreditAccount = contraCreditAccount,
            ContraDebitAccount = contraDebitAccount
        };
        return Result.Success(passiveTransaction);
    }

    public void UpdateDetails(
        string? debitAccount,
        string? creditAccount,
        string? contraCreditAccount,
        string? contraDebitAccount)
    {
        DebitAccount = debitAccount;
        CreditAccount = creditAccount;
        ContraCreditAccount = contraCreditAccount;
        ContraDebitAccount = contraDebitAccount;
    }
}
