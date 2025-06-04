using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Application.Abstractions.Data;
using Application.PensionRequirements.UpdatePensionRequirement;
using Application.PensionRequirements;

namespace Associate.Application.PensionRequirements;

internal sealed class UpdatePensionRequirementCommandHandler(
    IPensionRequirementRepository pensionrequirementRepository,
    IUnitOfWork unitOfWork,
    PensionRequirementCommandHandlerValidation validator)
    : ICommandHandler<UpdatePensionRequirementCommand>
{
    private const string Workflow = "Associate.PensionRequirement.UpdateValidation";

    public async Task<Result> Handle(UpdatePensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var existingPensionRequirement = await pensionrequirementRepository.GetAsync(request.PensionRequirementId, cancellationToken);
        
        var validationResult = await validator.ValidateRequestAsync(
                request,
                request.IdentificationType,
                request.Identification,
                Workflow,
                (cmd, activateResult) => new UpdatePensionRequirementValidationContext(cmd, activateResult, existingPensionRequirement!),
                cancellationToken);
        
        if (validationResult.IsFailure)
            return Result.Failure(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        existingPensionRequirement!.UpdateDetails(
            request.Status
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Inactivación Requisitos Pensión Exitosa");
    }
}