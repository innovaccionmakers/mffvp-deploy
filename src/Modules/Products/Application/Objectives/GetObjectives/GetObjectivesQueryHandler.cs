using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.IntegrationEvents.ClientValidation;
using People.IntegrationEvents.DocumentTypeValidation;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Rules;
using Products.Domain.Objectives;
using Products.Domain.Portfolios;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IObjectiveRepository objectiveRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    ICapRpcClient rpc) : IQueryHandler<GetObjectivesQuery, GetObjectivesResponse>
{
    private const string GetObjectivesValidationWorkflow = "Products.Objective.ValidateGetObjectives";

    public async Task<Result<GetObjectivesResponse>> Handle(GetObjectivesQuery request,
        CancellationToken cancellationToken)
    {
        var typeIdValidation = await rpc.CallAsync<
            GetDocumentTypeIdByCodeRequest,
            GetDocumentTypeIdByCodeResponse>(
            nameof(GetDocumentTypeIdByCodeRequest),
            new GetDocumentTypeIdByCodeRequest(request.TypeId),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!typeIdValidation.Succeeded)
            return Result.Failure<GetObjectivesResponse>(
                Error.Validation(
                    typeIdValidation.Code,
                    typeIdValidation.Message));

        var affiliateValidation = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            nameof(GetActivateIdByIdentificationRequest),
            new GetActivateIdByIdentificationRequest(request.TypeId, request.Identification),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        var affiliateExists = affiliateValidation.Succeeded;
        var affiliateId = affiliateValidation.ActivateId;

        IReadOnlyCollection<Objective> objectives = Array.Empty<Objective>();

        if (affiliateExists)
            objectives = await objectiveRepository
                .GetByAffiliateAsync(affiliateId!.Value, cancellationToken);

        var affiliateHasObjectives = objectives.Any();
        var affiliateHasActiveObjectives = objectives.Any(o => o.Status == "A");
        var affiliateHasInactiveObjectives = objectives.Any(o => o.Status == "I");

        var statusValueAccepted = request.Status switch
        {
            StatusType.A or StatusType.I or StatusType.T => true,
            _ => false
        };

        var validationContext = new
        {
            AffiliateExists = affiliateExists,
            StatusValueAccepted = statusValueAccepted,
            AffiliateHasObjectives = affiliateHasObjectives,
            AffiliateHasActiveObjectives = affiliateHasActiveObjectives,
            AffiliateHasInactiveObjectives = affiliateHasInactiveObjectives,
            RequestedStatus = request.Status.ToString()
        };

        var (ok, _, errors) = await ruleEvaluator.EvaluateAsync(
            GetObjectivesValidationWorkflow,
            validationContext,
            cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<GetObjectivesResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var filtered = request.Status switch
        {
            StatusType.A => objectives.Where(o => o.Status == "A"),
            StatusType.I => objectives.Where(o => o.Status == "I"),
            _ => objectives
        };

        var objectiveDtos = filtered
            .Select(o => new ObjectiveDto(
                o.ObjectiveId,
                o.ObjectiveTypeId.ToString(),
                o.Name,
                o.Alternative.AlternativeTypeId.ToString(),
                o.Status
            ))
            .ToList();

        return Result.Success(
            new GetObjectivesResponse(objectiveDtos));
    }
}