using Common.SharedKernel.Domain;
using Products.Domain.PlanFunds;

namespace Products.Domain.Plans;

public sealed class Plan : Entity
{
    public int PlanId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string HomologatedCode { get; private set; }

    public IReadOnlyCollection<PlanFund> PlanFunds { get; private set; } = new List<PlanFund>();

    private Plan()
    {
    }

    public static Result<Plan> Create(string name, string description, string homologatedCode)
    {
        var plan = new Plan
        {
            Name = name,
            Description = description,
            HomologatedCode = homologatedCode
        };
        plan.Raise(new PlanCreatedDomainEvent(plan.PlanId));
        return Result.Success(plan);
    }

    public void UpdateDetails(string newName, string newDescription, string newHomologatedCode)
    {
        Name = newName;
        Description = newDescription;
        HomologatedCode = newHomologatedCode;
    }
}