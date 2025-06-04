using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Objectives.GetObjectives;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.Services;

public sealed class ObjectiveReader(
    IObjectiveRepository repo,
    IConfigurationParameterRepository configRepo)
    : IObjectiveReader
{
    public async Task<ObjectiveValidationContext> BuildValidationContextAsync(
        bool affiliateFound,
        int? affiliateId,
        StatusType requested,
        CancellationToken ct)
    {
        bool any = false, active = false, inactive = false;

        if (affiliateFound && affiliateId is { } id)
        {
            any = await repo.AnyAsync(id, ct);

            switch (requested)
            {
                case StatusType.A:
                    active = await repo.AnyWithStatusAsync(id, "A", ct);
                    break;
                case StatusType.I:
                    inactive = await repo.AnyWithStatusAsync(id, "I", ct);
                    break;
                case StatusType.T:
                    (active, inactive) = await Task
                        .WhenAll(
                            repo.AnyWithStatusAsync(id, "A", ct),
                            repo.AnyWithStatusAsync(id, "I", ct))
                        .ContinueWith(t => (t.Result[0], t.Result[1]));
                    break;
            }
        }

        return new ObjectiveValidationContext
        {
            AffiliateExists = affiliateFound,
            RequestedStatusAccepted = requested is StatusType.A or StatusType.I or StatusType.T,
            AffiliateHasObjectives = any,
            AffiliateHasActive = active,
            AffiliateHasInactive = inactive,
            RequestedStatus = requested.ToString()
        };
    }

    public async Task<IReadOnlyList<ObjectiveDto>> ReadDtosAsync(
        int affiliateId, StatusType requested, CancellationToken ct)
    {
        var filter = requested switch
        {
            StatusType.A => "A",
            StatusType.I => "I",
            _ => null
        };

        var objectives = await repo.GetByAffiliateAsync(affiliateId, filter, ct);
        if (!objectives.Any()) return Array.Empty<ObjectiveDto>();

        var altIds = objectives.Select(o => o.Alternative.AlternativeTypeId).Distinct();
        var cfg = await configRepo.GetByIdsAsync(altIds, ct);
        var map = cfg.ToDictionary(p => p.ConfigurationParameterId, p => p.HomologationCode);

        return objectives
            .Select(o => new ObjectiveDto(
                o.ObjectiveId,
                o.ObjectiveTypeId.ToString(),
                o.Name,
                map.GetValueOrDefault(o.Alternative.AlternativeTypeId, string.Empty),
                o.Status))
            .ToList();
    }
}