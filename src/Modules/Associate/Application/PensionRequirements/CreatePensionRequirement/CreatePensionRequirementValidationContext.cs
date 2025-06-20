using Associate.Integrations.Activates.GetActivateId;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

namespace Application.PensionRequirements.CreatePensionRequirement
{
    public sealed class CreatePensionRequirementValidationContext
    {
        public CreatePensionRequirementCommand Request { get; }
        public GetActivateIdResponse ActivateResult { get; }
        public Guid DocumentType { get; }        
        public bool DocumentTypeExists { get; set; }

        public CreatePensionRequirementValidationContext(CreatePensionRequirementCommand request, GetActivateIdResponse activateResult, Guid documentType)
        {
            Request = request;
            ActivateResult = activateResult;
            DocumentType = documentType;
        }
    }
}