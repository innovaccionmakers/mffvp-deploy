using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;
public sealed record CreatePensionRequirementCommand(
    string IdentificationType,
    string Identification,
    DateTime StartDateReqPen,
    DateTime EndDateReqPen
) : ICommand;