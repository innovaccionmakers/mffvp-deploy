using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class ActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public bool ExistingActivate { get; }

    public ActivateValidationContext(
        CreateActivateCommand request,
        bool existingActivate)
    {
        Request = request;
        ExistingActivate = existingActivate;
    }
}