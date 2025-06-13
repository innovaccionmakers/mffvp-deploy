using Products.Domain.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Objectives.GetObjectives;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Domain;
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
                    active = await repo.AnyWithStatusAsync(id, Status.Active, ct);
                    break;
                case StatusType.I:
                    inactive = await repo.AnyWithStatusAsync(id, Status.Inactive, ct);
                    break;
                case StatusType.T:
                    (active, inactive) = await Task
                        .WhenAll(
                            repo.AnyWithStatusAsync(id, Status.Active, ct),
                            repo.AnyWithStatusAsync(id, Status.Inactive, ct))
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
        int affiliateId,
        StatusType requested,
        CancellationToken ct)
    {
        Status? statusFilter = requested switch
        {
            StatusType.A => Status.Active,
            StatusType.I => Status.Inactive,
            _ => null
        };

        var dtos = await repo.Query()
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId &&
                        (statusFilter == null || o.Status == statusFilter))
            .Select(o => new ObjectiveDto(
                o.ObjectiveId,
                o.ObjectiveTypeId.ToString(),
                o.Name,
                o.Alternative.PlanFund.PensionFund.Name,
                o.Alternative.PlanFund.Plan.Name,
                o.Alternative.HomologatedCode,
                o.Alternative.Name,
                o.Alternative.Portfolios
                    .Where(p => p.IsCollector)
                    .Select(p => p.Portfolio.Name)
                    .FirstOrDefault() ?? string.Empty,
                o.Status
            ))
            .ToListAsync(ct);

        return dtos;
    }
}