using Common.SharedKernel.Domain;

namespace Products.Domain.Objectives;

public sealed class Objective : Entity
{
    private Objective()
    {
    }

    public long ObjectiveId { get; private set; }
    public int ObjectiveTypeId { get; private set; }
    public int AffiliateId { get; private set; }
    public int AlternativeId { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public DateTime CreationDate { get; private set; }

    public static Result<Objective> Create(
        int objectiveTypeId, int affiliateId, int alternativeId, string name, string status, DateTime creationDate
    )
    {
        var objective = new Objective
        {
            ObjectiveId = default,

            ObjectiveTypeId = objectiveTypeId,
            AffiliateId = affiliateId,
            AlternativeId = alternativeId,
            Name = name,
            Status = status,
            CreationDate = creationDate
        };

        objective.Raise(new ObjectiveCreatedDomainEvent(objective.ObjectiveId));
        return Result.Success(objective);
    }

    public void UpdateDetails(
        int newObjectiveTypeId, int newAffiliateId, int newAlternativeId, string newName, string newStatus,
        DateTime newCreationDate
    )
    {
        ObjectiveTypeId = newObjectiveTypeId;
        AffiliateId = newAffiliateId;
        AlternativeId = newAlternativeId;
        Name = newName;
        Status = newStatus;
        CreationDate = newCreationDate;
    }
}