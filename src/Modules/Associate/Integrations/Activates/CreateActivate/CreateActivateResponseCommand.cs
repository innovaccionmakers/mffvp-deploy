using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.CreateActivate;

public sealed record CreateActivateResponseCommand(
    string DocumentType,
    string Identification,
    bool Pensioner,
    bool MeetsRequirements,
    DateTime ActivateDate
) : ICommand<ActivateResponse>;