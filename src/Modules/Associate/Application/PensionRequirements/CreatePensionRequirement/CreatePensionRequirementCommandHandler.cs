using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Associate.Application.Abstractions.Data;
using Associate.Integrations.Activates.GetActivateId;
using Application.PensionRequirements.CreatePensionRequirement;
using Associate.Application.Abstractions.Rules;
using Associate.Application.Abstractions;
using MediatR;

namespace Associate.Application.PensionRequirements.CreatePensionRequirement;


internal sealed class CreatePensionRequirementCommandHandler(
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IPensionRequirementRepository pensionrequirementRepository,
    IUnitOfWork unitOfWork,
    ISender sender)
    : ICommandHandler<CreatePensionRequirementCommand>
{
    private const string Workflow = "";

    public async Task<Result> Handle(CreatePensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var activateQuery = new GetActivateIdQuery(request.IdentificationType, request.Identification);
        var activateResult = await sender.Send(activateQuery, cancellationToken);

        if (activateResult.IsFailure)
            return Result.Failure(
                Error.Validation(activateResult.Error.Code ?? string.Empty, activateResult.Error.Description ?? string.Empty));

        var validationContext = new CreatePensionRequirementValidationContext(request);

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

        var result = PensionRequirement.Create(
            request.StartDateReqPen,
            request.EndDateReqPen,
            DateTime.UtcNow,
            "Activo",
            activateResult.Value.ActivateId
        );

        if (result.IsFailure)
        {
            return Result.Failure<PensionRequirementResponse>(result.Error);
        }

        var pensionrequirement = result.Value;

        pensionrequirementRepository.Insert(pensionrequirement);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}