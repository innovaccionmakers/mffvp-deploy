using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Associate.Integrations.PensionRequirements.CreatePensionRequirement;

public class CreatePensionRequirementRequestCommand(
    int ActivateId,
    DateTime? StartDate,
    DateTime? ExpirationDate,
    DateTime CreationDate,
    Status Status
) : ICommand<PensionRequirementResponse>;
