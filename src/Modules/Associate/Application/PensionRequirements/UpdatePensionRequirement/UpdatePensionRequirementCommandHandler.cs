using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Associate.Application.Abstractions.Data;
using Associate.Integrations.Activates.GetActivateId;
using Associate.Application.Abstractions.Rules;
using Associate.Application.Abstractions;
using Application.PensionRequirements.UpdatePensionRequirement;
using MediatR;

namespace Associate.Application.PensionRequirements;

internal sealed class UpdatePensionRequirementCommandHandler(
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IPensionRequirementRepository pensionrequirementRepository,
    IUnitOfWork unitOfWork,
    ISender sender)
    : ICommandHandler<UpdatePensionRequirementCommand>
{
    private const string Workflow = "";

    public async Task<Result> Handle(UpdatePensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await pensionrequirementRepository.GetAsync(request.PensionRequirementId, cancellationToken);
        var activateQuery = new GetActivateIdQuery(request.IdentificationType, request.Identification);
        var activateResult = await sender.Send(activateQuery, cancellationToken);
        var validationContext = new UpdatePensionRequirementValidationContext(request, activateResult.Value.ActivateId);

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure(
                Error.Validation(first.Code, first.Message));
        }
        entity.UpdateDetails(
            request.Status
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}