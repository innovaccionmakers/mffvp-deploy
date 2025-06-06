using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class CreateActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public Activate ExistingActivate { get; }
    public bool DocumentType { get; }

    public CreateActivateValidationContext(CreateActivateCommand request, Activate existingActivate, bool documentType)
    {
        Request = request;
        ExistingActivate = existingActivate;
        DocumentType = documentType;
    }
}