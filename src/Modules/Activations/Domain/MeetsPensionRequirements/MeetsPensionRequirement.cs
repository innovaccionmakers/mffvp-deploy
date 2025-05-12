using Common.SharedKernel.Domain;

namespace Activations.Domain.MeetsPensionRequirements;

public sealed class MeetsPensionRequirement : Entity
{
    private MeetsPensionRequirement()
    {
    }

    public int MeetsPensionRequirementId { get; private set; }
    public int AffiliateId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime CreationDate { get; private set; }
    public string State { get; private set; }

    public static Result<MeetsPensionRequirement> Create(
        DateTime startdate, DateTime expirationdate, DateTime creationdate, string state, int affiliates
    )
    {
        var meetspensionrequirement = new MeetsPensionRequirement
        {
            MeetsPensionRequirementId = new int(),
            AffiliateId = affiliates,
            StartDate = startdate,
            ExpirationDate = expirationdate,
            CreationDate = creationdate,
            State = state
        };
        meetspensionrequirement.Raise(
            new MeetsPensionRequirementCreatedDomainEvent(meetspensionrequirement.MeetsPensionRequirementId));
        return Result.Success(meetspensionrequirement);
    }

    public void UpdateDetails(
        int newActivationId, DateTime newStartDate, DateTime newExpirationDate, DateTime newCreationDate,
        string newState
    )
    {
        AffiliateId = newActivationId;
        StartDate = newStartDate;
        ExpirationDate = newExpirationDate;
        CreationDate = newCreationDate;
        State = newState;
    }
}