using Common.SharedKernel.Domain;
using Products.Domain.AlternativePortfolios;
using Products.Domain.Commissions;
using Products.Domain.PortfolioValuations;

namespace Products.Domain.Portfolios;

public sealed class Portfolio : Entity
{
    public int PortfolioId { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public int ModalityId { get; private set; }
    public decimal InitialMinimumAmount { get; private set; }
    public decimal AdditionalMinimumAmount { get; private set; }
    public DateTime CurrentDate { get; private set; }
    public int CommissionRateTypeId { get; private set; }
    public decimal CommissionPercentage { get; private set; }
    public string HomologatedCode { get; private set; }
    public Status Status { get; private set; }

    private readonly List<AlternativePortfolio> _alternatives = new();
    public IReadOnlyCollection<AlternativePortfolio> Alternatives => _alternatives;

    private readonly List<Commission> _commissions = new();
    public IReadOnlyCollection<Commission> Commissions => _commissions;

    private readonly List<PortfolioValuation> _portfolioValuations = new();
    public IReadOnlyCollection<PortfolioValuation> PortfolioValuations => _portfolioValuations;

    private Portfolio()
    {
    }

    public static Result<Portfolio> Create(
        string homologatedCode,
        string name,
        string shortName,
        int modalityId,
        decimal initialMinimumAmount,
        decimal additionalMinimumAmount,
        DateTime currentDate,
        int commissionRateTypeId,
        decimal commissionPercentage,
        Status status
    )
    {
        var portfolio = new Portfolio
        {
            PortfolioId = default,
            HomologatedCode = homologatedCode,
            Name = name,
            ShortName = shortName,
            ModalityId = modalityId,
            InitialMinimumAmount = initialMinimumAmount,
            AdditionalMinimumAmount = additionalMinimumAmount,
            CurrentDate = currentDate,
            CommissionRateTypeId = commissionRateTypeId,
            CommissionPercentage = commissionPercentage,
            Status = status
        };

        portfolio.Raise(new PortfolioCreatedDomainEvent(portfolio.PortfolioId));
        return Result.Success(portfolio);
    }

    public void UpdateDetails(
        string newHomologatedCode,
        string newName,
        string newShortName,
        int newModalityId,
        decimal newInitialMinimumAmount,
        decimal newAdditionalMinimumAmount,
        DateTime newCurrentDate,
        int newCommissionRateTypeId,
        decimal newCommissionPercentage,
        Status newStatus
    )
    {
        HomologatedCode = newHomologatedCode;
        Name = newName;
        ShortName = newShortName;
        ModalityId = newModalityId;
        InitialMinimumAmount = newInitialMinimumAmount;
        AdditionalMinimumAmount = newAdditionalMinimumAmount;
        CurrentDate = newCurrentDate;
        CommissionRateTypeId = newCommissionRateTypeId;
        CommissionPercentage = newCommissionPercentage;
        Status = newStatus;
    }
}