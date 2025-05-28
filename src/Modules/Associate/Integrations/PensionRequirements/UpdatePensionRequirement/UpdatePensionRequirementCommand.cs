using Common.SharedKernel.Application.Messaging;
using System;

namespace Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
public sealed record UpdatePensionRequirementCommand(
    int PensionRequirementId,
    DateTime NewAffiliateId,
    DateTime NewStartDate,
    DateTime NewExpirationDate,
    DateTime NewCreationDate,
    string NewStatus
) : ICommand<PensionRequirementResponse>;