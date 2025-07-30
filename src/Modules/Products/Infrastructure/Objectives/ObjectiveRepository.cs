using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Objectives;
using Products.Domain.Plans;
using Products.Infrastructure.Database;
using Products.Integrations.Objectives.GetObjectivesByAffiliate;
using System;

namespace Products.Infrastructure.Objectives;

internal sealed class ObjectiveRepository(ProductsDbContext context) : IObjectiveRepository
{
    public async Task AddAsync(Objective objective, CancellationToken ct = default)
    {
        await context.Objectives.AddAsync(objective, ct);
    }
    public async Task<IReadOnlyCollection<Objective>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Objectives.ToListAsync(cancellationToken);
    }

    public async Task<Objective?> GetAsync(int objectiveId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .SingleOrDefaultAsync(x => x.ObjectiveId == objectiveId, cancellationToken);
    }

    public async Task<Objective?> GetByIdAsync(int objectiveId, CancellationToken ct)
    {
        return await context.Objectives
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId, ct);
    }

    public async Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(
        int affiliateId, CancellationToken cancellationToken = default)
    {
        return await context.Objectives
            .AsNoTracking()
            .Include(o => o.Alternative)
            .Where(o => o.AffiliateId == affiliateId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(int affiliateId, CancellationToken ct = default)
    {
        return context.Objectives
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId)
            .AnyAsync(ct);
    }

    public Task<bool> AnyWithStatusAsync(int affiliateId, Status status, CancellationToken ct = default)
    {
        return context.Objectives
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId && o.Status == status)
            .AnyAsync(ct);
    }

    public async Task<IReadOnlyCollection<Objective>> GetByAffiliateAsync(
        int affiliateId,
        Status? status,
        CancellationToken ct = default)
    {
        var query = context.Objectives
            .Include(o => o.Alternative)
            .AsNoTracking()
            .Where(o => o.AffiliateId == affiliateId);

        if (status is not null)
            query = query.Where(o => o.Status == status);

        return await query.ToListAsync(ct);
    }
    public IQueryable<Objective> Query() => context.Objectives;

    public async Task<IReadOnlyCollection<AffiliateObjectiveQueryResponse>> GetAffiliateObjectivesByAffiliateIdAsync(
        int affiliateId,
        CancellationToken cancellationToken = default)
    {
        var result = await(from o in context.Objectives
                           join alt in context.Alternatives on o.AlternativeId equals alt.AlternativeId
                           join pf in context.PlanFunds on alt.PlanFundId equals pf.PlanFundId
                           join plan in context.Plans on pf.PlanId equals plan.PlanId
                           join fund in context.PensionFunds on pf.PensionFundId equals fund.PensionFundId
                           join com in context.Commercials on o.CommercialId equals com.CommercialId
                           join openOffice in context.Offices on o.OpeningOfficeId equals openOffice.OfficeId
                           join currOffice in context.Offices on o.CurrentOfficeId equals currOffice.OfficeId
                           join type in context.ConfigurationParameters on o.ObjectiveTypeId equals type.ConfigurationParameterId
                           where o.AffiliateId == affiliateId
                           select AffiliateObjectiveQueryResponse.Create(
                                o.ObjectiveId,
                                o.Name,
                                o.ObjectiveTypeId.ToString(),
                                type.Name,
                                type.HomologationCode,
                                plan.PlanId.ToString(),
                                plan.Name,
                                fund.PensionFundId.ToString(),
                                fund.ShortName,
                                alt.AlternativeId.ToString(),
                                alt.Name,
                                com.CommercialId.ToString(),
                                com.Name,
                                openOffice.OfficeId.ToString(),
                                openOffice.Name,
                                currOffice.OfficeId.ToString(),
                                currOffice.Name,
                                o.Status.ToString()
                            )
                        ).ToListAsync(cancellationToken);
        return result;
    }
}