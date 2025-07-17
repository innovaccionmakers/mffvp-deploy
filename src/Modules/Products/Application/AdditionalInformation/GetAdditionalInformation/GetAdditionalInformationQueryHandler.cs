using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Services.AdditionalInformation;
using Products.Domain.Objectives;
using Products.Integrations.AdditionalInformation;
using Common.SharedKernel.Application.Rules;
using Products.Application.Abstractions;

namespace Products.Application.AdditionalInformation.GetAdditionalInformation;

internal sealed class GetAdditionalInformationQueryHandler(
    IAdditionalInformationService service,
    IObjectiveRepository objectiveRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator)
    : IQueryHandler<GetAdditionalInformationQuery, IReadOnlyCollection<AdditionalInformationItem>>
{
    public async Task<Result<IReadOnlyCollection<AdditionalInformationItem>>> Handle(
        GetAdditionalInformationQuery request,
        CancellationToken cancellationToken)
    {
        var hasObjectives = await objectiveRepository.AnyAsync(request.AffiliateId, cancellationToken);

        var validationContext = new { AffiliateHasObjectives = hasObjectives };
        var (ok, _, errors) = await ruleEvaluator.EvaluateAsync(
            "Products.AdditionalInformation.Validation",
            validationContext,
            cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<IReadOnlyCollection<AdditionalInformationItem>>(
                Error.Validation(first.Code, first.Message));
        }
        var result = await service.GetInformationAsync(request.Pairs, cancellationToken);
        return Result.Success(result);
    }
}
