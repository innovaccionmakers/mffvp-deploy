using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Application.EventBus;

namespace Associate.Application.Activates.CreateActivate;

public sealed class ActivateValidationContext
{
    public CreateActivateCommand Request { get; }
    public PersonDataResponseEvent? Person { get; }
    public bool ExistingActivate { get; }

    public ActivateValidationContext(
        CreateActivateCommand request,
        PersonDataResponseEvent? person,
        bool existingActivate)
    {
        Request = request;
        Person = person;
        ExistingActivate = existingActivate;
    }
}