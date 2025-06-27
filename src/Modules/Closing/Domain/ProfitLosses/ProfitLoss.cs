using Common.SharedKernel.Domain;

namespace Closing.Domain.ProfitLosses;

public sealed class ProfitLoss : Entity
{
    public long ProfitLossId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public long ProfitLossConceptId { get; private set; }
    public decimal Amount { get; private set; }
    public string Source { get; private set; } = null!;
    public ProfitLossConcepts.ProfitLossConcept? ProfitLossConcept { get; private set; }

    private ProfitLoss()
    {
    }

    public static Result<ProfitLoss> Create(
        int portfolioId,
        DateTime processDate,
        DateTime effectiveDate,
        long profitLossConceptId,
        decimal amount,
        string source)
    {
        var profitLoss = new ProfitLoss
        {
            ProfitLossId = default,
            PortfolioId = portfolioId,
            ProcessDate = processDate,
            EffectiveDate = effectiveDate,
            ProfitLossConceptId = profitLossConceptId,
            Amount = amount,
            Source = source
        };

        return Result.Success(profitLoss);
    }

    public void UpdateDetails(
        int portfolioId,
        DateTime processDate,
        DateTime effectiveDate,
        long profitLossConceptId,
        decimal amount,
        string source)
    {
        PortfolioId = portfolioId;
        ProcessDate = processDate;
        EffectiveDate = effectiveDate;
        ProfitLossConceptId = profitLossConceptId;
        Amount = amount;
        Source = source;
    }
}