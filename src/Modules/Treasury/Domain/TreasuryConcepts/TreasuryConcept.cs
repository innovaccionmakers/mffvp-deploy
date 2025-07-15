using Common.SharedKernel.Domain;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Domain.TreasuryConcepts;

public sealed class TreasuryConcept : Entity
{
    private TreasuryConcept()
    {
    }

    public long Id { get; private set; }
    public string Concept { get; private set; }
    public IncomeExpenseNature Nature { get; private set; }
    public bool AllowsNegative { get; private set; }
    public bool AllowsExpense { get; private set; }
    public bool RequiresBankAccount { get; private set; }
    public bool RequiresCounterparty { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public string Observations { get; private set; }

    public ICollection<TreasuryMovement> TreasuryMovements { get; set; }

    public static Result<TreasuryConcept> Create(
        string concept,
        IncomeExpenseNature nature,
        bool allowsNegative,
        bool allowsExpense,
        bool requiresBankAccount,
        bool requiresCounterparty,
        DateTime processDate,
        string observations)
    {
        var treasuryConcept = new TreasuryConcept
        {
            Concept = concept,
            Nature = nature,
            AllowsNegative = allowsNegative,
            AllowsExpense = allowsExpense,
            RequiresBankAccount = requiresBankAccount,
            RequiresCounterparty = requiresCounterparty,
            ProcessDate = processDate,
            Observations = observations
        };

        return Result.Success(treasuryConcept);
    }
}