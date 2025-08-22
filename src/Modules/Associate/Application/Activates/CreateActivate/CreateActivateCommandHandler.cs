using Application.Activates;

using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Customers.IntegrationEvents.PersonValidation;

using MediatR;

namespace Associate.Application.Activates.CreateActivate;

internal sealed class CreateActivateCommandHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    IRpcClient rpc,
    ISender sender,
    ActivatesCommandHandlerValidation validator)
    : ICommandHandler<CreateActivateCommand>
{
    private const string Workflow = "Associate.Activates.CreateValidation";

    public async Task<Result> Handle(CreateActivateCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var validationResults = await validator.CreateActivateValidateRequestAsync(request, cancellationToken);

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationResults, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure(
                Error.Validation(first.Code, first.Message));
        }

        var personData = await rpc.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
            new PersonDataRequestEvent(request.DocumentType, request.Identification),
            cancellationToken);

        if (!personData.IsValid)
            return Result.Failure(
                Error.Validation(personData.Code ?? string.Empty, personData.Message ?? string.Empty));

        var result = Activate.Create(
            validationResults.DocumentType,
            request.Identification,
            request.Pensioner ?? false,
            request.MeetsPensionRequirements ?? false,
            DateTime.UtcNow
        );

        if (result.IsFailure)
            return Result.Failure(result.Error);

        var activate = result.Value;

        activateRepository.Insert(activate, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (request.MeetsPensionRequirements == true && request.Pensioner == false)
        {
            var CreatePensionRequirementCommand = new CreatePensionRequirementCommand(
                request.DocumentType,
                request.Identification,
                request.StartDateReqPen ?? DateTime.UtcNow,
                request.EndDateReqPen ?? DateTime.UtcNow
            );

            await sender.Send(CreatePensionRequirementCommand, cancellationToken);
        }

        return Result.Success("Activación causada Exitosamente");
    }
}