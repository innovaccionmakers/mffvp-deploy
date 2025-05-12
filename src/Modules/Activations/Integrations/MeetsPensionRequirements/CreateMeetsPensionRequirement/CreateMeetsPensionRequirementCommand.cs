using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;

public sealed record CreateMeetsPensionRequirementCommand(
    int AffiliateId,
    DateTime StartDate,
    DateTime ExpirationDate,
    DateTime CreationDate,
    string State
) : ICommand<MeetsPensionRequirementResponse>;