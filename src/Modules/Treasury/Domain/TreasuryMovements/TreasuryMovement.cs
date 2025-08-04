using Common.SharedKernel.Domain;
using System.Collections.ObjectModel;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;

namespace Treasury.Domain.TreasuryMovements;

public sealed class TreasuryMovement : Entity
{
    private readonly List<TreasuryMovement> _movements = new();

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
    public long EntityId { get; private set; }
    public long? CounterpartyId { get; private set; }

    public TreasuryConcept TreasuryConcept { get; set; }
    public BankAccount BankAccount { get; set; }
    public Issuer Entity { get; set; }
    public Issuer Counterparty { get; set; }

    public IReadOnlyCollection<TreasuryMovement> Movements => _movements.AsReadOnly();

    public static Result<TreasuryMovement> Create(
        int portfolioId,
        DateTime processDate,
        DateTime closingDate,
        long treasuryConceptId,
        decimal value,
        long? bankAccountId,
        long entityId,
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

    public void AddMovement(TreasuryMovement movement)
    {
        if (movement == null)
            throw new ArgumentNullException(nameof(movement));

        _movements.Add(movement);
    }

    public void AddMovements(IEnumerable<TreasuryMovement> movements)
    {
        if (movements == null)
            throw new ArgumentNullException(nameof(movements));

        _movements.AddRange(movements);
    }

    public void RemoveMovement(TreasuryMovement movement)
    {
        if (movement == null)
            throw new ArgumentNullException(nameof(movement));

        _movements.Remove(movement);
    }

    public void ClearMovements()
    {
        _movements.Clear();
    }
}