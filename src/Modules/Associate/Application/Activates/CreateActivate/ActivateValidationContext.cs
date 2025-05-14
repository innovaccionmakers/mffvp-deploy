using Associate.Integrations.Activates.CreateActivate;

namespace Associate.Application.Activates.CreateActivate;

public sealed class ActivateValidationContext
{
    public CreateActivateCommand Request { get; }    public bool IdType { get; }
    public bool Identification { get; }
    public bool Pensioner { get; }
    public bool Requirements { get; }
    public bool Dates { get; }
    public bool ExistingActivate { get; }

    public ActivateValidationContext(
        CreateActivateCommand request,
        bool idType,
        bool identification,
        bool pensioner,
        bool requirements,
        bool dates,
        bool existingActivate)
    {
        Request = request;
        IdType = idType;
        Identification = identification;
        Pensioner = pensioner;
        Requirements = requirements;
        Dates = dates;
        ExistingActivate = existingActivate;
    }
}