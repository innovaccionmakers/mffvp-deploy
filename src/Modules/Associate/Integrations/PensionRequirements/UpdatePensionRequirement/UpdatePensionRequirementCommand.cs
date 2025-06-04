using Common.SharedKernel.Application.Messaging;
using System;

namespace Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
public sealed record UpdatePensionRequirementCommand(
    string IdentificationType,
    string Identification,
    int PensionRequirementId,
    bool Status
) : ICommand;