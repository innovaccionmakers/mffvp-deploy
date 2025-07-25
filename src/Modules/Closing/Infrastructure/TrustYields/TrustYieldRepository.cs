using Closing.Domain.TrustYields;
using Closing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Closing.Infrastructure.TrustYields;

internal sealed class TrustYieldRepository(ClosingDbContext context) : ITrustYieldRepository
{
    public async Task<TrustYield?> GetByTrustAndDateAsync(int trustId, DateTime closingDateUtc, CancellationToken cancellationToken = default)
    {
        return await context.TrustYields
            .SingleOrDefaultAsync(x => x.TrustId == trustId && x.ClosingDate == closingDateUtc, cancellationToken);
    }

    public async Task InsertAsync(TrustYield trustYield, CancellationToken cancellationToken = default)
    {
        await context.TrustYields.AddAsync(trustYield, cancellationToken);
    }

    public void Update(TrustYield trustYield)
    {
        context.TrustYields.Update(trustYield);
    }

    public async Task<IReadOnlyCollection<TrustYield>> GetByPortfolioAndDateAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        return await context.TrustYields
            .Where(t => t.PortfolioId == portfolioId && t.ClosingDate.Date == closingDate.Date)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task UpsertAsync(YieldTrustSnapshot snapshot, CancellationToken ct)
    {
        var existing = await context
            .Set<TrustYield>()
            .FirstOrDefaultAsync(y =>
                y.TrustId == snapshot.TrustId &&
                y.ClosingDate.Date == snapshot.ClosingDate,
                ct);

        if (existing is null)
        {
            var result = TrustYield.Create(
                trustId: snapshot.TrustId,
                portfolioId: snapshot.PortfolioId,
                closingDate: snapshot.ClosingDate,
                participation: 0m,
                units: 0m,
                yieldAmount: 0m,
                preClosingBalance: snapshot.PreClosingBalance,
                closingBalance: 0m,
                income: 0m,
                expenses: 0m,
                commissions: 0m,
                cost: 0m,
                capital: snapshot.Capital,
                processDate: snapshot.ProcessDate,
                contingentRetention: snapshot.ContingentRetention,
                yieldRetention: 0m
            );

            if (result.IsSuccess)
                await context.AddAsync(result.Value, ct);
        }
        else
        {
            existing.UpdateDetails(
                trustId: existing.TrustId,
                portfolioId: existing.PortfolioId,
                closingDate: existing.ClosingDate,
                participation: existing.Participation,
                units: existing.Units,
                yieldAmount: existing.YieldAmount,
                preClosingBalance: snapshot.PreClosingBalance,
                closingBalance: existing.ClosingBalance,
                income: existing.Income,
                expenses: existing.Expenses,
                commissions: existing.Commissions,
                cost: existing.Cost,
                capital: snapshot.Capital,
                processDate: snapshot.ProcessDate,
                contingentRetention: snapshot.ContingentRetention,
                yieldRetention: existing.YieldRetention
            );
        }

        await context.SaveChangesAsync(ct);
    }
}

