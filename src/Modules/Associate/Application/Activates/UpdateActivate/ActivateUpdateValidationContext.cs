using Associate.Domain.Activates;
using Associate.Integrations.Activates.UpdateActivate;

namespace Associate.Application.Activates.UpdateActivate
{
    internal class ActivateUpdateValidationContext
    {
        public UpdateActivateCommand Request { get; }
        public Activate ExistingActivate { get; }
        public Guid IdentificationType { get; }

        public ActivateUpdateValidationContext(UpdateActivateCommand request, Activate existingActivate, Guid identificationType)
        {
            Request = request;
            ExistingActivate = existingActivate;
            IdentificationType = identificationType;
        }
    }
}
