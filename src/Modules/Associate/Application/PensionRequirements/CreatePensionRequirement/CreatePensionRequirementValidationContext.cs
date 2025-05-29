using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

namespace Application.PensionRequirements.CreatePensionRequirement
{
    public sealed class CreatePensionRequirementValidationContext
    {
        public CreatePensionRequirementCommand Request { get; }

        public CreatePensionRequirementValidationContext(CreatePensionRequirementCommand request)
        {
            Request = request;
        }
    }
}