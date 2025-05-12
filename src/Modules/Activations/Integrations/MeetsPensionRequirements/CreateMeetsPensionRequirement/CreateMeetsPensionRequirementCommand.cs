using Activations.Domain.Affiliates;
using Common.SharedKernel.Application.Messaging;
using System;

namespace Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
public sealed record CreateMeetsPensionRequirementCommand(
    int AffiliateId,
    DateTime StartDate,
    DateTime ExpirationDate,
    DateTime CreationDate,
    string State
) : ICommand<MeetsPensionRequirementResponse>;