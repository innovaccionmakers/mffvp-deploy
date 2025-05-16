using Associate.Domain.Clients;
using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class ActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public Client? Client { get; }
    public bool ExistingActivate { get; }

    public ActivateValidationContext(
        CreateActivateCommand request,
        Client? client,
        bool existingActivate)
    {
        Request = request;
        Client = client;
        ExistingActivate = existingActivate;
    }
}