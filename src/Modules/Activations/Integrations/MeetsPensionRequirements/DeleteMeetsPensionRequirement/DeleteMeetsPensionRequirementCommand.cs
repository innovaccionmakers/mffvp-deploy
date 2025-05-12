using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.MeetsPensionRequirements.DeleteMeetsPensionRequirement;
public sealed record DeleteMeetsPensionRequirementCommand(
    int MeetsPensionRequirementId
) : ICommand;