using Common.SharedKernel.Domain;

namespace Associate.Domain.PensionRequirements;

public sealed class PensionRequirement : Entity
{
    private DateTime? _startDate;
    private int? _activateId;
    public int PensionRequirementId { get; private set; }
    public int ActivateId { get => _activateId ?? 0; private set => _activateId = value; }
    public DateTime StartDate { get => _startDate ?? DateTime.UtcNow; private set => _startDate = value; }
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