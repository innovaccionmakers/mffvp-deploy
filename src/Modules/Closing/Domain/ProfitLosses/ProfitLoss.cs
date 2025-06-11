using Common.SharedKernel.Domain;

namespace Closing.Domain.ProfitLosses;

public sealed class ProfitLoss : Entity
{
    public long ProfitLossId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string Concept { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Source { get; private set; } = null!;

    private ProfitLoss()
    {
    }

    public static Result<ProfitLoss> Create(
        int portfolioId,
        DateTime processDate,
        DateTime effectiveDate,
        string concept,
        decimal amount,
        string source)
    {
        var profitLoss = new ProfitLoss
        {
            ProfitLossId = default,
            PortfolioId = portfolioId,
            ProcessDate = processDate,
            EffectiveDate = effectiveDate,
            Concept = concept,
            Amount = amount,
            Source = source
        };
        
        return Result.Success(profitLoss);
    }

    public void UpdateDetails(
        int portfolioId,
        DateTime processDate,
        DateTime effectiveDate,
        string concept,
        decimal amount,
        string source)
    {
        PortfolioId = portfolioId;
        ProcessDate = processDate;
        EffectiveDate = effectiveDate;
        Concept = concept;
        Amount = amount;
        Source = source;
    }
}