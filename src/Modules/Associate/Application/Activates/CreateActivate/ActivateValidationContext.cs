using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class ActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public Activate ExistingActivate { get; }

    public ActivateValidationContext(
        CreateActivateCommand request,
        Activate existingActivate)
    {
        Request = request;
        ExistingActivate = existingActivate;
    }
}