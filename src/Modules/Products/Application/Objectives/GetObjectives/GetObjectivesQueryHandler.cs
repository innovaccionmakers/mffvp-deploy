using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IAffiliateLocator affiliateLocator,
    IObjectiveReader objectiveReader,
    IGetObjectivesRules objectivesRules)
    : IQueryHandler<GetObjectivesQuery, IReadOnlyCollection<ObjectiveItem>>
{
    public async Task<Result<IReadOnlyCollection<ObjectiveItem>>> Handle(
        GetObjectivesQuery request,
        CancellationToken cancellationToken)
    {
        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.TypeId,
            HomologScope.Of<GetObjectivesQuery>(c => c.TypeId),
            cancellationToken);

        Result<int?> affiliateValidationResult = await affiliateLocator
            .FindAsync(request.TypeId, request.Identification, cancellationToken);
        if (!affiliateValidationResult.IsSuccess)
            return Result.Failure<IReadOnlyCollection<ObjectiveItem>>(affiliateValidationResult.Error!);

        var affiliateId = affiliateValidationResult.Value;
        var isAffiliateFound = affiliateId.HasValue;
        var docExists        = documentType is not null;

        var validationContext = await objectiveReader.BuildValidationContextAsync(
            isAffiliateFound,
            affiliateId,
            request.Status,
            docExists,
            cancellationToken);

        var rulesEvaluationResult = await objectivesRules
            .EvaluateAsync(validationContext, cancellationToken);
        if (!rulesEvaluationResult.IsSuccess)
            return Result.Failure<IReadOnlyCollection<ObjectiveItem>>(rulesEvaluationResult.Error!);

        var objectivesList = await objectiveReader.ReadDtosAsync(
            affiliateId.Value,
            request.Status,
            cancellationToken);

        var wrapper = objectivesList
            .Select(o => new ObjectiveItem(o))
            .ToList();

        return Result.Success<IReadOnlyCollection<ObjectiveItem>>(wrapper);
    }
}