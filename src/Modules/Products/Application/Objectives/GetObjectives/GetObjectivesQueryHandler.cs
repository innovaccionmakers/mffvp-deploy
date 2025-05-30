using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IDocumentTypeValidator docValidator,
    IAffiliateLocator affiliateLocator,
    IObjectiveReader objectiveReader,
    IGetObjectivesRules rules)
    : IQueryHandler<GetObjectivesQuery, GetObjectivesResponse>
{
    public async Task<Result<GetObjectivesResponse>> Handle(
        GetObjectivesQuery request, CancellationToken ct)
    {
        var docOk = await docValidator.EnsureExistsAsync(request.TypeId, ct);
        if (!docOk.IsSuccess)
            return Result.Failure<GetObjectivesResponse>(docOk.Error!);

        var (found, affiliateId) = await affiliateLocator.FindAsync(
            request.TypeId, request.Identification, ct);

        var ctx = await objectiveReader.BuildValidationContextAsync(
            found, affiliateId, request.Status, ct);
        var rulesOk = await rules.EvaluateAsync(ctx, ct);
        if (!rulesOk.IsSuccess)
            return Result.Failure<GetObjectivesResponse>(rulesOk.Error!);

        if (!found || affiliateId is null)
            return Result.Success(new GetObjectivesResponse([]));

        var dtos = await objectiveReader.ReadDtosAsync(
            affiliateId.Value, request.Status, ct);

        return Result.Success(new GetObjectivesResponse(dtos));
    }
}