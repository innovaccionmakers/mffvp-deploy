using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;

public class CreatePensionRequirementRequestCommand(
    int ActivateId,
    DateTime? StartDate,
    DateTime? ExpirationDate,
    DateTime CreationDate,
    string Status
) : ICommand<PensionRequirementResponse>;
