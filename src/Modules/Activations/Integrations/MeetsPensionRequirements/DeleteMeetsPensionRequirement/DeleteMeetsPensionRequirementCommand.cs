using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.MeetsPensionRequirements.DeleteMeetsPensionRequirement;

public sealed record DeleteMeetsPensionRequirementCommand(
    int MeetsPensionRequirementId
) : ICommand;