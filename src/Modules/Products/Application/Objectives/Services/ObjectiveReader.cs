using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
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
        bool documentTypeExists,
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
                    var flags = await repo.Query()
                        .Where(o => o.AffiliateId == id &&
                                    (o.Status == Status.Active || o.Status == Status.Inactive))
                        .GroupBy(_ => 1)
                        .Select(g => new {
                            HasActive   = g.Any(x => x.Status == Status.Active),
                            HasInactive = g.Any(x => x.Status == Status.Inactive)
                        })
                        .FirstOrDefaultAsync(ct);

                    active   = flags?.HasActive   ?? false;
                    inactive = flags?.HasInactive ?? false;
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
            RequestedStatus = requested.ToString(),
            DocumentTypeExists = documentTypeExists
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
            .OrderBy(o => o.ObjectiveId)
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
                    .Select(p => p.Portfolio.HomologatedCode)
                    .FirstOrDefault() ?? string.Empty,
                o.Alternative.Portfolios
                    .Where(p => p.IsCollector)
                    .Select(p => p.Portfolio.Name)
                    .FirstOrDefault() ?? string.Empty,
                o.Status == Status.Active ? "Activo" : "Inactivo"
            ))
            .ToListAsync(ct);

        return dtos;
    }
}