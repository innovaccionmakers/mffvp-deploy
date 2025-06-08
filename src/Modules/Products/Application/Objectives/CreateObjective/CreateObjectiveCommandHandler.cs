using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Rules;
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
    IDocumentTypeValidator documentTypeValidator,
    IAlternativeRepository alternativeRepository,
    IAffiliateLocator affiliateLocator,
    ICommercialRepository commercialRepository,
    IOfficeRepository officeRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateObjectiveCommand, ObjectiveResponse>
{
    private const string ObjectiveCreationValidationWorkflow = "Products.CreateObjective.Validation";

    public async Task<Result<ObjectiveResponse>> Handle(CreateObjectiveCommand request,
        CancellationToken cancellationToken)
    {
        var alternative =
            await alternativeRepository.GetByHomologatedCodeAsync(request.AlternativeId, cancellationToken);

        var objectiveType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.ObjectiveType,
            HomologScope.Of<CreateObjectiveCommand>(c => c.ObjectiveType),
            cancellationToken);

        var docResult = await documentTypeValidator.EnsureExistsAsync(request.IdType, cancellationToken);
        if (!docResult.IsSuccess)
            return Result.Failure<ObjectiveResponse>(docResult.Error!);

        var affiliateResult = await affiliateLocator.FindAsync(
            request.IdType, request.Identification, cancellationToken);

        if (!affiliateResult.IsSuccess)
            return Result.Failure<ObjectiveResponse>(affiliateResult.Error!);

        var clientAffiliated = affiliateResult.Value.HasValue;

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
            "ACTIVO",
            DateTime.UtcNow,
            commercial!,
            offices[request.OpeningOffice].OfficeId,
            offices[request.CurrentOffice].OfficeId,
            0m
        );

        if (!creationResult.IsSuccess)
            return Result.Failure<ObjectiveResponse>(creationResult.Error!);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var response = new ObjectiveResponse(creationResult.Value.ObjectiveId);
        return Result.Success(response,"Objetivo creado Exitosamente");
    }
}