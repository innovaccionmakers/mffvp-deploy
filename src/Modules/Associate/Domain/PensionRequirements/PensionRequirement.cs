using Common.SharedKernel.Domain;
using Associate.Domain.Activates;

namespace Associate.Domain.PensionRequirements;
public sealed class PensionRequirement : Entity
{
    public int PensionRequirementId { get; private set; }
    public int AffiliateId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime CreationDate { get; private set; }
    public string Status { get; private set; }

    private PensionRequirement() { }

    public static Result<PensionRequirement> Create(
        DateTime startdate, DateTime expirationdate, DateTime creationdate, string status, Activate activate
    )
    {
        var pensionrequirement = new PensionRequirement
        {
                PensionRequirementId = new int(),
                AffiliateId = activate.ActivateId,
                StartDate = startdate,
                ExpirationDate = expirationdate,
                CreationDate = creationdate,
                Status = status,
        };
        pensionrequirement.Raise(new PensionRequirementCreatedDomainEvent(pensionrequirement.PensionRequirementId));
        return Result.Success(pensionrequirement);
    }

    public void UpdateDetails(
        int newAffiliateId, DateTime newStartDate, DateTime newExpirationDate, DateTime newCreationDate, string newStatus
    )
    {
        AffiliateId = newAffiliateId;
        StartDate = newStartDate;
        ExpirationDate = newExpirationDate;
        CreationDate = newCreationDate;
        Status = newStatus;
    }
}