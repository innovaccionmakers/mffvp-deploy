using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;

using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Services.External;
using Products.Domain.Commercials;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Domain.Offices;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.UpdateObjective;

namespace Products.Application.Objectives.UpdateObjective;

internal sealed class UpdateObjectiveCommandHandler(
    IObjectiveRepository objectiveRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    ICommercialRepository commercialRepository,
    IOfficeRepository officeRepository,
    IObjectivesValidationTrusts objectivesValidationTrusts,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateObjectiveCommand>
{
    private const string RequiredFieldsWorkflow = "Products.UpdateObjective.RequiredFields";
    private const string ObjectiveUpdateValidationWorkflow = "Products.UpdateObjective.Validation";

    public async Task<Result> Handle(
        UpdateObjectiveCommand request,
        CancellationToken cancellationToken)
    {

        var requiredContext = new
        {
            request.ObjectiveId,
            HasFieldsToUpdate = request.HasAnyFieldToUpdate()
        };

        var (requiredOk, _, requiredErrors) = await ruleEvaluator.EvaluateAsync(
            RequiredFieldsWorkflow,
            requiredContext,
            cancellationToken);

        if (!requiredOk)
        {
            var firstError = requiredErrors.First();
            return Result.Failure<ObjectiveResponse>(
                Error.Validation(firstError.Code, firstError.Message));
        }

        var objective = await objectiveRepository.GetAsync(request.ObjectiveId, cancellationToken);
        if (objective is null)
            return Result.Failure<ObjectiveResponse>(ObjectiveErrors.NotFound(request.ObjectiveId));

        ConfigurationParameter? objectiveType = null;
        Commercial? commercial = null;
        IReadOnlyDictionary<string, Office>? offices = null;

        //cargar y validar ObjectiveType si está presente
        if (!string.IsNullOrEmpty(request.ObjectiveType))
        {
            objectiveType = await configurationParameterRepository.GetByCodeAndScopeAsync(
                request.ObjectiveType,
                HomologScope.Of<UpdateObjectiveCommand>(c => c.ObjectiveType),
                cancellationToken);
        }

        //cargar y validar Commercial si está presente
        if (!string.IsNullOrEmpty(request.Commercial))
        {
            commercial = await commercialRepository.GetByHomologatedCodeAsync(
                request.Commercial,
                cancellationToken);
        }

        //cargar y validar Offices si alguna está presente
        var officeCodes = new[] { request.OpeningOffice, request.CurrentOffice }
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct()
            .ToArray();

        if (officeCodes.Any())
        {
            offices = await officeRepository.GetByHomologatedCodesAsync(officeCodes, cancellationToken);
        }

        //validar con el módulo de fideicomisos
        var trustValidation = await objectivesValidationTrusts.ValidateAsync(request.ObjectiveId, request.Status,
            cancellationToken);

        var contextValidation = new
        {
            ObjectiveExists = objective != null,
            RequestedStatus = request.Status,
            IsStatusValid = request.Status is "A" or "I",
            HasTrust = trustValidation.Value.HasTrust,
            HasTrustWithBalance = trustValidation.Value.HasTrustWithBalance,
            RequestedOpeningOffice = request.OpeningOffice,
            CurrentOpeningOffice = objective.OpeningOfficeId.ToString(),

            RequestedObjectiveType = request.ObjectiveType,
            ObjectiveTypeExists = objectiveType is not null,
            OpeningOfficeExists = string.IsNullOrWhiteSpace(request.OpeningOffice) ? true : (offices?.ContainsKey(request.OpeningOffice) ?? false),
            RequestedCurrentOffice = request.CurrentOffice,
            CurrentOfficeExists = string.IsNullOrWhiteSpace(request.CurrentOffice) ? true : (offices?.ContainsKey(request.CurrentOffice) ?? false),
            RequestedCommercial = request.Commercial,
            CommercialExists = commercial is not null
        };

        var (isValid, _, errors) = await ruleEvaluator.EvaluateAsync(
            ObjectiveUpdateValidationWorkflow,
            contextValidation,
            cancellationToken);

        if (!isValid)
        {
            var error = errors.First();
            return Result.Failure<ObjectiveResponse>(
                Error.Validation(error.Code, error.Message));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        objective.UpdateDetails(
            objectiveType?.ConfigurationParameterId ?? objective.ObjectiveTypeId,
            objective.AffiliateId,
            objective.AlternativeId,
            request.ObjectiveName ?? objective.Name,
            request.Status?.ToUpperInvariant() switch
            {
                "A" => Status.Active,
                "I" => Status.Inactive,
                _ => objective.Status
            },
            objective.CreationDate,
            commercial?.CommercialId ?? objective.CommercialId,
            !string.IsNullOrWhiteSpace(request.OpeningOffice) ? offices?.GetValueOrDefault(request.OpeningOffice)?.OfficeId ?? objective.OpeningOfficeId : objective.OpeningOfficeId,
            !string.IsNullOrWhiteSpace(request.CurrentOffice) ? offices?.GetValueOrDefault(request.CurrentOffice)?.OfficeId ?? objective.CurrentOfficeId : objective.CurrentOfficeId,
            objective.Balance
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Actualización del Objetivo Exitosa");
       
    }
}