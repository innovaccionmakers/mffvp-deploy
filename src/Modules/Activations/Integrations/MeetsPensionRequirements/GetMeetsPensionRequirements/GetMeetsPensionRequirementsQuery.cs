using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirements;
public sealed record GetMeetsPensionRequirementsQuery() : IQuery<IReadOnlyCollection<MeetsPensionRequirementResponse>>;