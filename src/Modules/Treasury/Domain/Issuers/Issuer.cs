using Common.SharedKernel.Domain;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Domain.Issuers;

public sealed class Issuer : Entity
{
    private Issuer()
    {
    }

    public long Id { get; private set; }
    public string IssuerCode { get; private set; }
    public string Description { get; private set; }
    public float Nit { get; private set; }
    public int Digit { get; private set; }
    public string HomologatedCode { get; private set; }

    public ICollection<BankAccount> BankAccounts { get; set; }
    public ICollection<TreasuryMovement> TreasuryMovementsAsEntity { get; set; }
    public ICollection<TreasuryMovement> TreasuryMovementsAsCounterparty { get; set; }


    public static Result<Issuer> Create(
        string issuerCode,
        string description,
        float nit,
        int digit,
        string homologatedCode)
    {
        var issuer = new Issuer
        {
            IssuerCode = issuerCode,
            Description = description,
            Nit = nit,
            Digit = digit,
            HomologatedCode = homologatedCode
        };

        return Result.Success(issuer);
    }
}