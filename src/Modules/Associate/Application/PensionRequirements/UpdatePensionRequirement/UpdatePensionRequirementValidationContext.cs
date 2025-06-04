using Associate.Domain.PensionRequirements;
using Associate.Integrations.Activates.GetActivateId;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

namespace Application.PensionRequirements.UpdatePensionRequirement
{
    public class UpdatePensionRequirementValidationContext
    {
        public UpdatePensionRequirementCommand Request { get; }
        public GetActivateIdResponse ActivateResult { get; }
        public PensionRequirement ExistingPensionRequirement { get; }


        public UpdatePensionRequirementValidationContext(UpdatePensionRequirementCommand request, GetActivateIdResponse activateResult, PensionRequirement existingPensionRequirement)
        {
            ActivateResult = activateResult;
            Request = request;
            ExistingPensionRequirement = existingPensionRequirement;
        }
    }
}