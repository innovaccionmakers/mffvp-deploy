using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IDocumentTypeValidator documentTypeValidator,
    IAffiliateLocator affiliateLocator,
    IObjectiveReader objectiveReader,
    IGetObjectivesRules objectivesRules)
    : IQueryHandler<GetObjectivesQuery, GetObjectivesResponse>
{
    public async Task<Result<GetObjectivesResponse>> Handle(
        GetObjectivesQuery request,
        CancellationToken cancellationToken)
    {
        var documentValidationResult = await documentTypeValidator
            .EnsureExistsAsync(request.TypeId, cancellationToken);
        if (!documentValidationResult.IsSuccess)
            return Result.Failure<GetObjectivesResponse>(documentValidationResult.Error!);

        var affiliateValidationResult = await affiliateLocator
            .FindAsync(request.TypeId, request.Identification, cancellationToken);
        if (!affiliateValidationResult.IsSuccess)
            return Result.Failure<GetObjectivesResponse>(affiliateValidationResult.Error!);

        var affiliateId = affiliateValidationResult.Value;
        var isAffiliateFound = affiliateId.HasValue;

        var validationContext = await objectiveReader.BuildValidationContextAsync(
            isAffiliateFound,
            affiliateId,
            request.Status,
            cancellationToken);

        var rulesEvaluationResult = await objectivesRules
            .EvaluateAsync(validationContext, cancellationToken);
        if (!rulesEvaluationResult.IsSuccess)
            return Result.Failure<GetObjectivesResponse>(rulesEvaluationResult.Error!);

        if (!isAffiliateFound)
            return Result.Success(
                new GetObjectivesResponse(Array.Empty<ObjectiveDto>()));

        var objectivesList = await objectiveReader.ReadDtosAsync(
            affiliateId.Value,
            request.Status,
            cancellationToken);

        return Result.Success(new GetObjectivesResponse(objectivesList));
    }
}