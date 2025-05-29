using Common.SharedKernel.Domain;
using Products.Domain.AlternativePortfolios;

namespace Products.Domain.Portfolios;

public sealed class Portfolio : Entity
{
    public int PortfolioId { get; private set; }
    public string StandardCode { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public int ModalityId { get; private set; }
    public decimal InitialMinimumAmount { get; private set; }
    
    private readonly List<AlternativePortfolio> _alternatives = [];
    public IReadOnlyCollection<AlternativePortfolio> Alternatives => _alternatives;

    private Portfolio()
    {
    }

    public static Result<Portfolio> Create(
        string standardCode, string name, string shortName, int modalityId, decimal initialMinimumAmount
    )
    {
        var portfolio = new Portfolio
        {
            PortfolioId = default,

            StandardCode = standardCode,
            Name = name,
            ShortName = shortName,
            ModalityId = modalityId,
            InitialMinimumAmount = initialMinimumAmount
        };

        portfolio.Raise(new PortfolioCreatedDomainEvent(portfolio.PortfolioId));
        return Result.Success(portfolio);
    }

    public void UpdateDetails(
        string newStandardCode, string newName, string newShortName, int newModalityId, decimal newInitialMinimumAmount
    )
    {
        StandardCode = newStandardCode;
        Name = newName;
        ShortName = newShortName;
        ModalityId = newModalityId;
        InitialMinimumAmount = newInitialMinimumAmount;
    }
}