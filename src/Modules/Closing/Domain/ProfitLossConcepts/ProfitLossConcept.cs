using Common.SharedKernel.Domain;

namespace Closing.Domain.ProfitLossConcepts;

public sealed class ProfitLossConcept : Entity
{
    public long ProfitLossConceptId { get; private set; }
    public string Concept { get; private set; } = null!;
    public string Nature { get; private set; } = null!;
    public bool AllowNegative { get; private set; }

    private ProfitLossConcept()
    {
    }

    public static Result<ProfitLossConcept> Create(string concept, string nature, bool allowNegative)
    {
        var entity = new ProfitLossConcept
        {
            ProfitLossConceptId = default,
            Concept = concept,
            Nature = nature,
            AllowNegative = allowNegative
        };
        
        return Result.Success(entity);
    }

    public void UpdateDetails(string concept, string nature, bool allowNegative)
    {
        Concept = concept;
        Nature = nature;
        AllowNegative = allowNegative;
    }
}