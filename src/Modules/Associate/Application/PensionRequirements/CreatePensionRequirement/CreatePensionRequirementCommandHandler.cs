using Application.PensionRequirements;

using Associate.Application.Abstractions.Data;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Associate.Application.PensionRequirements.CreatePensionRequirement;


internal sealed class CreatePensionRequirementCommandHandler(
    IPensionRequirementRepository pensionrequirementRepository,
    IUnitOfWork unitOfWork,
    PensionRequirementCommandHandlerValidation validator)
    : ICommandHandler<CreatePensionRequirementCommand>
{
    private const string Workflow = "Associate.PensionRequirement.CreateValidation";
    public async Task<Result> Handle(CreatePensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var validationResult = await validator.CreatePensionRequirementValidateRequestAsync(request, Workflow, cancellationToken);
        
        if (validationResult.IsFailure)
            return Result.Failure(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        var activateId = validationResult.Value.ActivateResult.ActivateId;

        var result = PensionRequirement.Create(
            request.StartDateReqPen ?? DateTime.UtcNow,
            request.EndDateReqPen ?? DateTime.UtcNow,
            DateTime.UtcNow,
            Status.Active,
            activateId
        );

        if (result.IsFailure)
            return Result.Failure<PensionRequirementResponse>(result.Error);

        await pensionrequirementRepository.DeactivateExistingRequirementsAsync(activateId, cancellationToken);

        pensionrequirementRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Registro de Requisitos de Pensión Exitoso");
    }
}