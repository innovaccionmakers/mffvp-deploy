using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.CreateActivate;

public sealed record CreateActivateCommand(
    string IdentificationType,
    string Identification,
    bool Pensioner,
    bool MeetsPensionRequirements,
    DateTime? StartDateReqPen,
    DateTime? EndDateReqPen
) : ICommand<ActivateResponse>;