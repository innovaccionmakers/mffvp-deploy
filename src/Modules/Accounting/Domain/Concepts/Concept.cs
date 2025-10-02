using Common.SharedKernel.Domain;

namespace Accounting.Domain.Concepts;

public class Concept : Entity
{
    public long ConceptId { get; private set; }
    public int PortfolioId { get; private set; }
    public string Name { get; private set; }
    public string? DebitAccount { get; private set; }
    public string? CreditAccount { get; private set; }

    private Concept()
    {
    }

    public static Result<Concept> Create(
        int portfolioId,
        string name,
        string? debitAccount,
        string? creditAccount)
    {
        var concept = new Concept
        {
            ConceptId = default,
            PortfolioId = portfolioId,
            Name = name,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount
        };
        return Result.Success(concept);
    }

    public void UpdateDetails(
        int portfolioId,
        string name,
        string? debitAccount,
        string? creditAccount)
    {
        PortfolioId = portfolioId;
        Name = name;
        DebitAccount = debitAccount;
        CreditAccount = creditAccount;
    }
}
