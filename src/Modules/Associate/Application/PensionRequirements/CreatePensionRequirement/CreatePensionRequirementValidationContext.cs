using Associate.Integrations.Activates.GetActivateId;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

namespace Application.PensionRequirements.CreatePensionRequirement
{
    public sealed class CreatePensionRequirementValidationContext
    {
        public CreatePensionRequirementCommand Request { get; }
        public GetActivateIdResponse ActivateResult { get; }
        public Guid IdentificationType { get; }

        public CreatePensionRequirementValidationContext(CreatePensionRequirementCommand request, GetActivateIdResponse activateResult, Guid identificationType)
        {
            Request = request;
            ActivateResult = activateResult;
            IdentificationType = identificationType;
        }
    }
}