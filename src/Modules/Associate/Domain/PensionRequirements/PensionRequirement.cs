using Common.SharedKernel.Domain;
using Associate.Domain.Activates;

namespace Associate.Domain.PensionRequirements;

public sealed class PensionRequirement : Entity
{
    public int PensionRequirementId { get; private set; }
    public int ActivateId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime CreationDate { get; private set; }
    public Status Status { get; private set; }

    private PensionRequirement() { }

    public static Result<PensionRequirement> Create(
        DateTime startdate, DateTime expirationdate, DateTime creationdate, Status status, int activateId
    )
    {
        var pensionrequirement = new PensionRequirement
        {
            PensionRequirementId = new int(),
            ActivateId = activateId,
            StartDate = startdate,
            ExpirationDate = expirationdate,
            CreationDate = creationdate,
            Status = status,
        };
        pensionrequirement.Raise(new PensionRequirementCreatedDomainEvent(pensionrequirement.PensionRequirementId));
        return Result.Success(pensionrequirement);
    }

    public void UpdateDetails(Status status)
    {
        Status = status;
    }
}