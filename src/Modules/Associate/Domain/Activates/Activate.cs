using Common.SharedKernel.Domain;

namespace Associate.Domain.Activates;

public sealed class Activate : Entity
{
    private Activate()
    {
    }

    public int ActivateId { get; private set; }
    public string IdentificationType { get; private set; }
    public string Identification { get; private set; }
    public bool Pensioner { get; private set; }
    public bool MeetsPensionRequirements { get; private set; }
    public DateTime ActivateDate { get; private set; }

    public static Result<Activate> Create(
        string identificationtype, string identification, bool pensioner, bool meetsrequirements,
        DateTime activatedate
    )
    {
        var activate = new Activate
        {
            ActivateId = new int(),
            IdentificationType = identificationtype,
            Identification = identification,
            Pensioner = pensioner,
            MeetsPensionRequirements = meetsrequirements,
            ActivateDate = activatedate
        };
        activate.Raise(new ActivateCreatedDomainEvent(activate.ActivateId));
        return Result.Success(activate);
    }

    public void UpdateDetails(
        string newIdentificationType, string newIdentification, bool newPensioner, bool newMeetsRequirements,
        DateTime newActivateDate
    )
    {
        IdentificationType = newIdentificationType;
        Identification = newIdentification;
        Pensioner = newPensioner;
        MeetsPensionRequirements = newMeetsRequirements;
        ActivateDate = newActivateDate;
    }
}