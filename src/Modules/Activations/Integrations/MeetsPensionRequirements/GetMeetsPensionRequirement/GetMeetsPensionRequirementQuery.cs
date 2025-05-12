using Common.SharedKernel.Application.Messaging;

namespace Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirement;

public sealed record GetMeetsPensionRequirementQuery(
    int MeetsPensionRequirementId
) : IQuery<MeetsPensionRequirementResponse>;