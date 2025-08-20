using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.Application.Abstractions;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IAffiliateLocator affiliateLocator,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    IObjectiveReader objectiveReader,
    IGetObjectivesRules objectivesRules)
    : IQueryHandler<GetObjectivesQuery, IReadOnlyCollection<ObjectiveItem>>
{
    private const string RequiredFieldsWorkflow = "Products.Objective.RequiredFieldsGetObjectives";
    
    public async Task<Result<IReadOnlyCollection<ObjectiveItem>>> Handle(
        GetObjectivesQuery request,
        CancellationToken ct)
    {
        var requiredContext = new
        {
            request.TypeId,
            request.Identification,
            request.Status
        };
        
        var (requiredOk, _, requiredErrors) =
            await ruleEvaluator.EvaluateAsync(RequiredFieldsWorkflow,
                requiredContext,
                ct);

        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<IReadOnlyCollection<ObjectiveItem>>(
                Error.Validation(first.Code, first.Message));
        }
        
        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.TypeId,
            HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
            ct);

        var documentTypeExists = documentType is not null;

        var affiliateRaw = documentTypeExists
            ? await affiliateLocator.FindAsync(request.TypeId, request.Identification, ct)
            : Result.Success<int?>(null);

        if (!affiliateRaw.IsSuccess)
            return Result.Failure<IReadOnlyCollection<ObjectiveItem>>(affiliateRaw.Error!);

        var affiliateId = affiliateRaw.Value;
        var affiliateFound = affiliateId.HasValue;

        var validationContext = await objectiveReader.BuildValidationContextAsync(
            affiliateFound,
            affiliateId,
            request.Status,
            documentTypeExists,
            ct);

        var rulesResult = await objectivesRules.EvaluateAsync(validationContext, ct);
        if (!rulesResult.IsSuccess)
            return Result.Failure<IReadOnlyCollection<ObjectiveItem>>(rulesResult.Error!);

        var dtos = await objectiveReader.ReadDtosAsync(
            affiliateId!.Value,
            request.Status,
            ct);

        var wrapper = dtos.Select(d => new ObjectiveItem(d)).ToList();
        return Result.Success<IReadOnlyCollection<ObjectiveItem>>(wrapper);
    }
}