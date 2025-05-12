using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirement;
public sealed record GetMeetsPensionRequirementQuery(
    int MeetsPensionRequirementId
) : IQuery<MeetsPensionRequirementResponse>;