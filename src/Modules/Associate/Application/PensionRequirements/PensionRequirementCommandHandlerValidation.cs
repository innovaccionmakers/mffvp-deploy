
using Associate.Integrations.Activates.GetActivateId;
using Common.SharedKernel.Domain;
using MediatR;
using Common.SharedKernel.Application.Rules;
using Associate.Application.Abstractions;
using Application.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Attributes;
using Application.PensionRequirements.UpdatePensionRequirement;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Domain.PensionRequirements;

namespace Application.PensionRequirements;

public class PensionRequirementCommandHandlerValidation(
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IConfigurationParameterRepository configurationParameterRepository,
    ISender sender)
{
    public async Task<Result<UpdatePensionRequirementValidationContext>> UpdatePensionRequirementValidationContext(
            UpdatePensionRequirementCommand request,
            string Workflow,
            PensionRequirement existingPensionRequirement,
            CancellationToken cancellationToken)
    {
        GetActivateIdResponse? activateData = null;
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType,
            HomologScope.Of<UpdatePensionRequirementCommand>(c => c.DocumentType),
            cancellationToken);

        var docTypeUuid = configurationParameter?.Uuid ?? Guid.Empty;

        var validationResult = await ValidateRequestAsync(
        request,
        request.DocumentType,
        request.Identification,
        Workflow,
        (cmd, activateResult) =>
        {
            activateData = activateResult;
            return new UpdatePensionRequirementValidationContext(cmd, activateResult, existingPensionRequirement!, docTypeUuid)
            {
                DocumentTypeExists = configurationParameter != null
            };
        },
        cancellationToken);

        if (validationResult.IsFailure)
            return Result.Failure<UpdatePensionRequirementValidationContext>(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        var validationContext = new UpdatePensionRequirementValidationContext(request, activateData!, existingPensionRequirement!, docTypeUuid)
        {
            DocumentTypeExists = configurationParameter != null
        };

        var (isValid, _, ruleErrors) =
            await ruleEvaluator.EvaluateAsync(Workflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = ruleErrors
                .OrderByDescending(r => r.Code)
                .First();

            return Result.Failure<UpdatePensionRequirementValidationContext>(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));
        }

        return Result.Success(validationContext);
    }

    public async Task<Result<CreatePensionRequirementValidationContext>> CreatePensionRequirementValidateRequestAsync(
        CreatePensionRequirementCommand request,
        string Workflow,
        CancellationToken cancellationToken)
    {
        GetActivateIdResponse? activateData = null;
        var configurationParameter = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType,
            HomologScope.Of<CreatePensionRequirementCommand>(c => c.DocumentType),
            cancellationToken);

        var docTypeUuid = configurationParameter?.Uuid ?? Guid.Empty;

        var validationResult = await ValidateRequestAsync(
                request,
                request.DocumentType,
                request.Identification,
                Workflow,
                (cmd, activateResult) =>
                {
                    activateData = activateResult;
                    return new CreatePensionRequirementValidationContext(cmd, activateResult, docTypeUuid)
                    {
                        DocumentTypeExists = configurationParameter != null
                    };
                },
                cancellationToken);

        if (validationResult.IsFailure)
            return Result.Failure<CreatePensionRequirementValidationContext>(
                Error.Validation(validationResult.Error.Code ?? string.Empty, validationResult.Error.Description ?? string.Empty));

        return Result.Success(new CreatePensionRequirementValidationContext(request, activateData!, docTypeUuid)
        {
            DocumentTypeExists = configurationParameter != null
        });

    }

    public async Task<Result> ValidateRequestAsync<TCommand>(
            TCommand request,
            string identificationType,
            string identification,
            string Workflow,
            Func<TCommand, GetActivateIdResponse, object> validationContextFactory,
            CancellationToken cancellationToken)
    {
        var activateQuery = new GetActivateIdQuery(identificationType, identification);
        var activateResult = await sender.Send(activateQuery, cancellationToken);

        if (activateResult.IsFailure)
            return Result.Failure(
                Error.Validation(activateResult.Error.Code ?? string.Empty, activateResult.Error.Description ?? string.Empty));

        var validationContext = validationContextFactory(request, activateResult.Value);

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

        return Result.Success();
    }
}
