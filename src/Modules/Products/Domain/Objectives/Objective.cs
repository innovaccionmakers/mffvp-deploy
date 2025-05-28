using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.Offices;
using Products.Domain.Cities;

namespace Products.Domain.Objectives;

public sealed class Objective : Entity
{
    public int ObjectiveId { get; private set; }
    public int ObjectiveTypeId { get; private set; }
    public int AffiliateId { get; private set; }
    public int AlternativeId { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public DateTime CreationDate { get; private set; }
    public int CommercialId { get; private set; }
    public int OfficeId { get; private set; }
    public int CityId { get; private set; }
    
    public Alternative Alternative { get; private set; } = null!;

    private Objective()
    {
    }

    public static Result<Objective> Create(
        int objectiveTypeId, int affiliateId, string name, string status, DateTime creationDate,
        Alternative alternative, Commercial commercial, Office office, City city
    )
    {
        var objective = new Objective
        {
            ObjectiveId = default,
            ObjectiveTypeId = objectiveTypeId,
            AffiliateId = affiliateId,
            AlternativeId = alternative.AlternativeId,
            Name = name,
            Status = status,
            CreationDate = creationDate,
            CommercialId = commercial.CommercialId,
            OfficeId = office.OfficeId,
            CityId = city.CityId
        };

        objective.Raise(new ObjectiveCreatedDomainEvent(objective.ObjectiveId));
        return Result.Success(objective);
    }

    public void UpdateDetails(
        int newObjectiveTypeId, int newAffiliateId, int newAlternativeId, string newName, string newStatus,
        DateTime newCreationDate, int newCommercialId, int newOfficeId, int newCityId
    )
    {
        ObjectiveTypeId = newObjectiveTypeId;
        AffiliateId = newAffiliateId;
        AlternativeId = newAlternativeId;
        Name = newName;
        Status = newStatus;
        CreationDate = newCreationDate;
        CommercialId = newCommercialId;
        OfficeId = newOfficeId;
        CityId = newCityId;
    }
}