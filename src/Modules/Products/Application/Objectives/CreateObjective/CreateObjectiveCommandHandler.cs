using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Services.External;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Domain.Offices;
using Products.Integrations.Objectives.CreateObjective;

namespace Products.Application.Objectives.CreateObjective;

internal sealed class CreateObjectiveCommandHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IAlternativeRepository alternativeRepository,
    IObjectiveRepository objectiveRepository,
    IAffiliateLocator affiliateLocator,
    ICommercialRepository commercialRepository,
    IOfficeRepository officeRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateObjectiveCommand, ObjectiveResponse>
{
    private const string RequiredFieldsWorkflow  = "Products.CreateObjective.RequiredFields";
    private const string ObjectiveCreationValidationWorkflow = "Products.CreateObjective.Validation";

    public async Task<Result<ObjectiveResponse>> Handle(CreateObjectiveCommand request,
        CancellationToken cancellationToken)
    {
        var requiredContext = new
        {
            request.IdType,
            request.Identification,
            request.AlternativeId,
            request.ObjectiveType,
            request.ObjectiveName,
            request.OpeningOffice,
            request.CurrentOffice,
            request.Commercial
        };

        var (requiredOk, _, requiredErrors) =
            await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow,
                requiredContext,
                cancellationToken);

        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<ObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }
        
        var alternative =
            await alternativeRepository.GetByHomologatedCodeAsync(request.AlternativeId, cancellationToken);

        var objectiveType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.ObjectiveType,
            HomologScope.Of<CreateObjectiveCommand>(c => c.ObjectiveType),
            cancellationToken);

        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.IdType,
            HomologScope.Of<CreateObjectiveCommand>(c => c.IdType),
            cancellationToken);

        var affiliateResult = Result.Success<int?>(null);
        var clientAffiliated = false;

        if (documentType is not null)
        {
            affiliateResult = await affiliateLocator.FindAsync(
                request.IdType, request.Identification, cancellationToken);

            if (!affiliateResult.IsSuccess)
                return Result.Failure<ObjectiveResponse>(affiliateResult.Error!);

            clientAffiliated = affiliateResult.Value.HasValue;
        }

        var officeCodes = new[]
            {
                request.OpeningOffice,
                request.CurrentOffice
            }
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct()
            .ToArray();

        var offices = await officeRepository
            .GetByHomologatedCodesAsync(officeCodes, cancellationToken);

        var commercial = await commercialRepository
            .GetByHomologatedCodeAsync(request.Commercial, cancellationToken);

        var validationContext = new
        {
            DocumentTypeExists = documentType is not null,
            AlternativeIdExists = alternative is not null,
            ObjectiveTypeExists = objectiveType is not null,
            ClientAffiliated = clientAffiliated,

            OpeningOfficeExists = offices.ContainsKey(request.OpeningOffice),
            CurrentOfficeExists = offices.ContainsKey(request.CurrentOffice),

            CommercialExists = commercial is not null
        };

        var (rulesOk, _, ruleErrors) = await ruleEvaluator
            .EvaluateAsync(ObjectiveCreationValidationWorkflow,
                validationContext,
                cancellationToken);

        if (!rulesOk)
        {
            var first = ruleErrors.First();
            return Result.Failure<ObjectiveResponse>(
                Error.Validation(first.Code, first.Message));
        }

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var creationResult = Objective.Create(
            objectiveType!.ConfigurationParameterId,
            affiliateResult.Value!.Value,
            alternative!,
            request.ObjectiveName,
            Status.Active,
            DateTime.UtcNow,
            commercial!,
            offices[request.OpeningOffice].OfficeId,
            offices[request.CurrentOffice].OfficeId,
            0m
        );

        if (!creationResult.IsSuccess)
            return Result.Failure<ObjectiveResponse>(creationResult.Error!);
        
        await objectiveRepository.AddAsync(creationResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new ObjectiveResponse(creationResult.Value.ObjectiveId);
        return Result.Success(response, "Objetivo creado Exitosamente");
    }
}