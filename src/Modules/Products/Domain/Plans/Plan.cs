using Common.SharedKernel.Domain;

namespace Products.Domain.Plans;

public sealed class Plan : Entity
{
    private Plan()
    {
    }

    public long PlanId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public static Result<Plan> Create(
        string name, string description
    )
    {
        var plan = new Plan
        {
            PlanId = default,

            Name = name,
            Description = description
        };

        plan.Raise(new PlanCreatedDomainEvent(plan.PlanId));
        return Result.Success(plan);
    }

    public void UpdateDetails(
        string newName, string newDescription
    )
    {
        Name = newName;
        Description = newDescription;
    }
}