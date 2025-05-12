using Common.SharedKernel.Domain;

namespace Activations.Domain.MeetsPensionRequirements;

public static class MeetsPensionRequirementErrors
{
    public static Error NotFound(int meetspensionrequirementId)
    {
        return Error.NotFound(
            "MeetsPensionRequirement.NotFound",
            $"The meetspensionrequirement with identifier {meetspensionrequirementId} was not found"
        );
    }
}