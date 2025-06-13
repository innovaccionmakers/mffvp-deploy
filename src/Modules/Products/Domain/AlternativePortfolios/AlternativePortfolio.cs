using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Domain.Portfolios;

namespace Products.Domain.AlternativePortfolios;

public sealed class AlternativePortfolio : Entity
{
    public int AlternativePortfolioId { get; private set; }
    public int AlternativeId { get; private set; }
    public int PortfolioId { get; private set; }
    public bool IsCollector { get; private set; }
    public Status Status { get; private set; }

    public Alternative Alternative { get; private set; }
    public Portfolio Portfolio { get; private set; }

    private AlternativePortfolio()
    {
    }

    public static Result<AlternativePortfolio> Create(
        Status status, Alternative alternative, Portfolio portfolio, bool isCollector
    )
    {
        var alternativePortfolio = new AlternativePortfolio
        {
            AlternativePortfolioId = default,
            AlternativeId = alternative.AlternativeId,
            PortfolioId = portfolio.PortfolioId,
            Status = status,
            IsCollector = isCollector
        };

        alternativePortfolio.Raise(
            new AlternativePortfolioCreatedDomainEvent(alternativePortfolio.AlternativePortfolioId));
        return Result.Success(alternativePortfolio);
    }

    public void UpdateDetails(
        int newAlternativeId, int newPortfolioId, Status newStatus, bool isCollector
    )
    {
        AlternativeId = newAlternativeId;
        PortfolioId = newPortfolioId;
        Status = newStatus;
        IsCollector = isCollector;
    }
}