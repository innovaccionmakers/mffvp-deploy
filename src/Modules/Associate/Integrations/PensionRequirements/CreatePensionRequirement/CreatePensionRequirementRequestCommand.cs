using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;

public class CreatePensionRequirementRequestCommand(
    int ActivateId,
    DateTime? StartDate,
    DateTime? ExpirationDate,
    DateTime CreationDate,
    Status Status
) : ICommand<PensionRequirementResponse>;
