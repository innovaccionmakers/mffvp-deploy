using Common.SharedKernel.Domain;

namespace Associate.Domain.PensionRequirements;
public static class PensionRequirementErrors
{
    public static Error NotFound(int pensionrequirementId) =>
        Error.NotFound(
            "PensionRequirement.NotFound",
            $"The pensionrequirement with identifier {pensionrequirementId} was not found"
        );
}