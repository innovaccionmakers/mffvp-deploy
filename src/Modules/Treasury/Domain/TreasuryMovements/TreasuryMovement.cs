using Common.SharedKernel.Domain;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;

namespace Treasury.Domain.TreasuryMovements;

public sealed class TreasuryMovement : Entity
{
    private TreasuryMovement()
    {
    }

    public long Id { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public long TreasuryConceptId { get; private set; }
    public decimal Value { get; private set; }
    public long? BankAccountId { get; private set; }
    public long? EntityId { get; private set; }
    public long? CounterpartyId { get; private set; }

    public TreasuryConcept TreasuryConcept { get; set; }
    public BankAccount BankAccount { get; set; }
    public Issuer Entity { get; set; }
    public Issuer Counterparty { get; set; }

    public static Result<TreasuryMovement> Create(
        int portfolioId,
        DateTime processDate,
        DateTime closingDate,
        long treasuryConceptId,
        decimal value,
        long? bankAccountId,
        long? entityId,
        long? counterpartyId)
    {
        var treasuryMovement = new TreasuryMovement
        {
            PortfolioId = portfolioId,
            ProcessDate = processDate,
            ClosingDate = closingDate,
            TreasuryConceptId = treasuryConceptId,
            Value = value,
            BankAccountId = bankAccountId,
            EntityId = entityId,
            CounterpartyId = counterpartyId
        };

        return Result.Success(treasuryMovement);
    }
}