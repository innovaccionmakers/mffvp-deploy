using Associate.Domain.Activates;
using Associate.Integrations.Activates.UpdateActivate;

namespace Associate.Application.Activates.UpdateActivate
{
    internal class ActivateUpdateValidationContext
    {
        public UpdateActivateCommand Request { get; }
        public Activate ExistingActivate { get; }
        public Guid DocumentType { get; }

        public ActivateUpdateValidationContext(UpdateActivateCommand request, Activate existingActivate, Guid documentType)
        {
            Request = request;
            ExistingActivate = existingActivate;
            DocumentType = documentType;
        }
    }
}
