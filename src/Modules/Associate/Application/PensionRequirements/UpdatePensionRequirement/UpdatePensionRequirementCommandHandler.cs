using Application.PensionRequirements;

using Associate.Application.Abstractions.Data;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using System.Data.Common;

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
        var existingPensionRequirement = await pensionrequirementRepository.GetAsync(request.PensionRequirementId ?? 0, cancellationToken);
        var validationResult = await validator.UpdatePensionRequirementValidationContext(request, Workflow, existingPensionRequirement!, cancellationToken);

        if (validationResult.IsFailure)
            return Result.Failure(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        existingPensionRequirement!.UpdateDetails(
            (bool)request.Status! ? Status.Active : Status.Inactive
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Inactivación Requisitos Pensión Exitosa");
    }
}