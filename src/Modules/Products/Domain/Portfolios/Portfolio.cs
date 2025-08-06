using Common.SharedKernel.Domain;
using Products.Domain.AlternativePortfolios;
using Products.Domain.Commissions;

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
    public Guid CommissionRateTypeId { get; private set; }
    public decimal CommissionPercentage { get; private set; }
    public string HomologatedCode { get; private set; }
    public int VerificationDigit { get; private set; }
    public string PortfolioNIT { get; private set; }
    public string NitApprovedPortfolio { get; private set; }
    public Guid RiskProfile { get; private set; }
    public int SFCBusinessCode { get; private set; }
    public Guid Custodian { get; private set; }
    public Guid Qualifier { get; private set; }
    public Guid Rating { get; private set; }
    public Guid RatingType { get; private set; }
    public DateTime LastRatingDate { get; private set; }
    public Guid AdviceClassification { get; private set; }
    public int MaxParticipationPercentage { get; private set; }
    public decimal MinimumVirPercentage { get; private set; }
    public decimal PartialVirPercentage { get; private set; }
    public int AgileWithdrawalPercentageProtectedBalance { get; private set; }
    public int WithdrawalPercentageProtectedBalance { get; private set; }
    public string AllowsAgileWithdrawal { get; private set; }
    public int PermanencePeriod { get; private set; }
    public int PenaltyPercentage { get; private set; }
    public DateTime OperationsStartDate { get; private set; }
    public DateTime PortfolioExpiryDate { get; private set; }
    public Guid IndustryClassification { get; private set; }
    public Status Status { get; private set; }

    private readonly List<AlternativePortfolio> _alternatives = new();
    public IReadOnlyCollection<AlternativePortfolio> Alternatives => _alternatives;

    private readonly List<Commission> _commissions = new();
    public IReadOnlyCollection<Commission> Commissions => _commissions;

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
        Guid commissionRateTypeId,
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
        Guid newCommissionRateTypeId,
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