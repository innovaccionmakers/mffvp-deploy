using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.Affiliates.CreateActivation;

public sealed record CreateActivationCommand(
    string IdentificationType,
    string Identification,
    bool Pensioner,
    bool MeetsPensionRequirements,
    DateTime? StartDateReqPen,
    DateTime? EndDateReqPen
) : ICommand<AffiliateResponse>;