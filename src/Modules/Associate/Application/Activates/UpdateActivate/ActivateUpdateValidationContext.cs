using Associate.Domain.Activates;
using Associate.Integrations.Activates.UpdateActivate;

namespace Associate.Application.Activates.UpdateActivate
{
    public class UpdateActivateValidationContext
    {
        public UpdateActivateCommand Request { get; }
        public Activate ExistingActivate { get; }
        public bool DocumentTypeExists { get; set; }

        public UpdateActivateValidationContext(UpdateActivateCommand request, Activate existingActivate)
        {
            Request = request;
            ExistingActivate = existingActivate;
        }
    }
}
