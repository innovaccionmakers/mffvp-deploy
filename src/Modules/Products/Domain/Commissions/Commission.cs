using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.Domain.AccumulatedCommissions;
using Products.Domain.Portfolios;

namespace Products.Domain.Commissions;

public sealed class Commission : Entity
{
    public int CommissionId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public string Concept { get; private set; }
    public string Modality { get; private set; }
    public string CommissionType { get; private set; }
    public string Period { get; private set; }
    public string CalculationBase { get; private set; }
    public string CalculationRule { get; private set; }
    public Status Status { get; private set; }

    public Portfolio Portfolio { get; private set; }

    private readonly List<AccumulatedCommission> _accumulatedCommissions = new();
    public IReadOnlyCollection<AccumulatedCommission> AccumulatedCommissions => _accumulatedCommissions;

    private Commission()
    {
    }

    public static Result<Commission> Create(
        int portfolioId,
        DateTime processDate,
        string concept,
        string modality,
        string commissionType,
        string period,
        string calculationBase,
        string calculationRule,
        Status status = Status.Active)
    {
        var commission = new Commission
        {
            PortfolioId = portfolioId,
            ProcessDate = processDate,
            Concept = concept,
            Modality = modality,
            CommissionType = commissionType,
            Period = period,
            CalculationBase = calculationBase,
            CalculationRule = calculationRule,
            Status = status
        };

        return Result.Success(commission);
    }

    public void UpdateDetails(
        DateTime newProcessDate,
        string newConcept,
        string newModality,
        string newCommissionType,
        string newPeriod,
        string newCalculationBase,
        string newCalculationRule,
        Status newStatus)
    {
        ProcessDate = newProcessDate;
        Concept = newConcept;
        Modality = newModality;
        CommissionType = newCommissionType;
        Period = newPeriod;
        CalculationBase = newCalculationBase;
        CalculationRule = newCalculationRule;
        Status = newStatus;
    }
}