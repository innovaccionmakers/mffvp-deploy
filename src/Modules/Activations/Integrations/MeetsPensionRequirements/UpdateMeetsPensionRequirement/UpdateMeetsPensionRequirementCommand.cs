using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;

public sealed record UpdateMeetsPensionRequirementCommand(
    int MeetsPensionRequirementId,
    int NewAffiliateId,
    DateTime NewStartDate,
    DateTime NewExpirationDate,
    DateTime NewCreationDate,
    string NewState
) : ICommand<MeetsPensionRequirementResponse>;