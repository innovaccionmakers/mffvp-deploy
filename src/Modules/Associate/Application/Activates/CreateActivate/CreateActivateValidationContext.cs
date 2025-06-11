using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class CreateActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public Activate ExistingActivate { get; }
    public Guid IdentificationType { get; }

    public CreateActivateValidationContext(CreateActivateCommand request, Activate existingActivate, Guid identificationType)
    {
        Request = request;
        ExistingActivate = existingActivate;
        IdentificationType = identificationType;
    }
}