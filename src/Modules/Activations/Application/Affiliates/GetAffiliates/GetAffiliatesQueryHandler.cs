using Activations.Domain.Affiliates;
using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.GetAffiliates;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Activations.Application.Affiliates.GetAffiliates;

internal sealed class GetAffiliatesQueryHandler(
    IAffiliateRepository affiliateRepository)
    : IQueryHandler<GetAffiliatesQuery, IReadOnlyCollection<AffiliateResponse>>
{
    public async Task<Result<IReadOnlyCollection<AffiliateResponse>>> Handle(GetAffiliatesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await affiliateRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new AffiliateResponse(
                e.AffiliateId,
                e.IdentificationType,
                e.Identification,
                e.Pensioner,
                e.MeetsRequirements,
                e.ActivationDate))
            .ToList();

        return Result.Success<IReadOnlyCollection<AffiliateResponse>>(response);
    }
}