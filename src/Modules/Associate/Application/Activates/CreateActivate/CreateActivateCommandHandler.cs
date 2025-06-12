using Associate.Domain.ConfigurationParameters;
using Associate.Application.Abstractions.Data;
using Common.SharedKernel.Application.Rules;
using Associate.Domain.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Customers.IntegrationEvents.PersonValidation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Application.Abstractions;
using MediatR;
using Common.SharedKernel.Application.Attributes;

namespace Associate.Application.Activates.CreateActivate;

internal sealed class CreateActivateCommandHandler(
    IActivateRepository activateRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    ICapRpcClient rpc,
    ISender sender,
    IConfigurationParameterRepository configurationParameterRepository)
    : ICommandHandler<CreateActivateCommand>
{
    private const string Workflow = "Associate.Activates.CreateValidation";

    public async Task<Result> Handle(CreateActivateCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.IdentificationType, HomologScope.Of<CreateActivateCommand>(c => c.IdentificationType), cancellationToken);
        Guid uuid = configurationParameter == null ? new Guid() : configurationParameter.Uuid;

        Activate? existingActivate = await activateRepository.GetByIdTypeAndNumber(uuid, request.Identification, cancellationToken);
        
        var personData = await rpc.CallAsync<
            PersonDataRequestEvent,
            GetPersonValidationResponse>(
            nameof(PersonDataRequestEvent),
            new PersonDataRequestEvent(request.IdentificationType, request.Identification),
            TimeSpan.FromSeconds(30),
            cancellationToken);

        if (!personData.IsValid)
            return Result.Failure(
                Error.Validation(personData.Code ?? string.Empty, personData.Message ?? string.Empty));

        var validationContext = new CreateActivateValidationContext(request, existingActivate!, uuid);

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

        var result = Activate.Create(
            configurationParameter.Uuid,
            request.Identification,
            request.Pensioner,
            request.MeetsPensionRequirements ?? false,
            DateTime.UtcNow
        );

        if (result.IsFailure)
            return Result.Failure(result.Error);

        var activate = result.Value;

        activateRepository.Insert(activate, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (request.MeetsPensionRequirements == true)
        {
            var CreatePensionRequirementCommand = new CreatePensionRequirementRequestCommand(
                activate.ActivateId,
                request.StartDateReqPen,
                request.EndDateReqPen,
                DateTime.UtcNow,
                "Activo"
            );

            await sender.Send(CreatePensionRequirementCommand, cancellationToken);
        }

        return Result.Success("Activación causada Exitosamente");
    }
}