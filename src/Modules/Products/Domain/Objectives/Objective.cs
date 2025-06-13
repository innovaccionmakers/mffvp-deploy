using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;

namespace Products.Domain.Objectives;

public sealed class Objective : Entity
{
    public int ObjectiveId { get; private set; }
    public int ObjectiveTypeId { get; private set; }
    public int AffiliateId { get; private set; }
    public int AlternativeId { get; private set; }
    public string Name { get; private set; }
    public DateTime CreationDate { get; private set; }
    public int CommercialId { get; private set; }
    public int OpeningOfficeId { get; private set; }
    public int CurrentOfficeId { get; private set; }
    public decimal Balance { get; private set; }
    public Status Status { get; private set; }

    public Alternative Alternative { get; private set; } = null!;
    public Commercial Commercial { get; private set; } = null!;

    private Objective()
    {
    }

    public static Result<Objective> Create(
        int objectiveTypeId,
        int affiliateId,
        Alternative alternative,
        string name,
        Status status,
        DateTime creationDate,
        Commercial commercial,
        int openingOfficeId,
        int currentOfficeId,
        decimal balance
    )
    {
        var objective = new Objective
        {
            ObjectiveTypeId = objectiveTypeId,
            AffiliateId = affiliateId,
            AlternativeId = alternative.AlternativeId,
            Alternative = alternative,
            Name = name,
            Status = status,
            CreationDate = creationDate,
            CommercialId = commercial.CommercialId,
            Commercial = commercial,
            OpeningOfficeId = openingOfficeId,
            CurrentOfficeId = currentOfficeId,
            Balance = balance
        };

        objective.Raise(new ObjectiveCreatedDomainEvent(objective.ObjectiveId));
        return Result.Success(objective);
    }

    public void UpdateDetails(
        int objectiveTypeId,
        int affiliateId,
        int alternativeId,
        string name,
        Status status,
        DateTime creationDate,
        int commercialId,
        int openingOfficeId,
        int currentOfficeId,
        decimal balance
    )
    {
        ObjectiveTypeId = objectiveTypeId;
        AffiliateId = affiliateId;
        AlternativeId = alternativeId;
        Name = name;
        Status = status;
        CreationDate = creationDate;
        CommercialId = commercialId;
        OpeningOfficeId = openingOfficeId;
        CurrentOfficeId = currentOfficeId;
        Balance = balance;
    }
}