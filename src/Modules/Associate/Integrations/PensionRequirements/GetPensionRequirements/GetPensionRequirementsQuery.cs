using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Associate.Integrations.PensionRequirements.GetPensionRequirements;
public sealed record GetPensionRequirementsQuery() : IQuery<IReadOnlyCollection<PensionRequirementResponse>>;