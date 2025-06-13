using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Domain.Plans;
using Products.Domain.PensionFunds;

namespace Products.Domain.PlanFunds;

public sealed class PlanFund : Entity
{
    public int PlanFundId { get; private set; }
    public int PlanId { get; private set; }
    public int PensionFundId { get; private set; }
    public Status Status { get; private set; }

    public Plan Plan { get; private set; } = null!;
    public PensionFund PensionFund { get; private set; } = null!;

    private readonly List<Alternative> _alternatives = new();
    public IReadOnlyCollection<Alternative> Alternatives => _alternatives;

    private PlanFund()
    {
    }

    public static Result<PlanFund> Create(Plan plan, PensionFund pensionFund, Status status)
    {
        var pf = new PlanFund
        {
            PlanId = plan.PlanId,
            PensionFundId = pensionFund.PensionFundId,
            Plan = plan,
            PensionFund = pensionFund,
            Status = status
        };
        return Result.Success(pf);
    }

    public void UpdateDetails(int planId, int pensionFundId, Status status)
    {
        PlanId = planId;
        PensionFundId = pensionFundId;
        Status = status;
    }
}