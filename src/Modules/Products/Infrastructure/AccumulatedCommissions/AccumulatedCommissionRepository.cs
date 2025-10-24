using Microsoft.EntityFrameworkCore;
using Npgsql;
using Products.Domain.AccumulatedCommissions;
using Products.Infrastructure.Database;
using System.Data;

namespace Products.Infrastructure.AccumulatedCommissions;
internal sealed class AccumulatedCommissionRepository(
    ProductsDbContext context
) : IAccumulatedCommissionRepository
{
    public async Task<AccumulatedCommission?> GetByPortfolioAndCommissionAsync(
        int portfolioId,
        int commissionId,
        CancellationToken cancellationToken = default)
    {
        return await context
            .AccumulatedCommissions
            .SingleOrDefaultAsync(x =>
                x.PortfolioId == portfolioId &&
                x.CommissionId == commissionId,
                cancellationToken);
    }

    public async Task AddAsync(
        AccumulatedCommission commission,
        CancellationToken cancellationToken)
    {
        await context.AccumulatedCommissions.AddAsync(commission, cancellationToken);
    }

    public async Task<bool> UpsertAsync(
      int portfolioId,
      int commissionId,
      DateTime closingDateEventUtc,
      decimal dailyAmount,
      CancellationToken cancellationToken)
    {
        // Normaliza a UTC
        var closeUtc = closingDateEventUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(closingDateEventUtc, DateTimeKind.Utc)
            : closingDateEventUtc.ToUniversalTime();


        await using var tx = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(@p,@c);",
            new[]
            {
            new NpgsqlParameter("p", portfolioId),
            new NpgsqlParameter("c", commissionId),
            },
            cancellationToken);

        // 2) UPDATE set-based solo si la fecha almacenada es menor a la del evento
        var updatedRows = await context.Set<AccumulatedCommission>()
            .Where(x => x.PortfolioId == portfolioId
                     && x.CommissionId == commissionId
                     && x.CloseDate < closeUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.AccumulatedValue, x => x.AccumulatedValue + dailyAmount)
                .SetProperty(x => x.PendingValue,
                             x => EF.Functions.Greatest((x.AccumulatedValue + dailyAmount) - x.PaidValue, 0m))
                .SetProperty(x => x.CloseDate, x => closeUtc)
                .SetProperty(x => x.ProcessDate, x => DateTime.UtcNow),
                cancellationToken);

        if (updatedRows > 0)
        {
            await tx.CommitAsync(cancellationToken);
            return true; // aplicado por UPDATE
        }

        // 3) Si no actualizó, ¿existe una fila para (portfolio, comisión)?
        var exists = await context.Set<AccumulatedCommission>()
            .TagWith("[AccumulatedCommissionRepository_Upsert_UpdateIfNewer]")
            .AnyAsync(x => x.PortfolioId == portfolioId
                        && x.CommissionId == commissionId,
                      cancellationToken);

        if (!exists)
        {
            // 4) INSERT
            var unpaidSentinelUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            var nowUtc = DateTime.UtcNow;

            var result = AccumulatedCommission.Create(
                portfolioId: portfolioId,
                commissionId: commissionId,
                accumulatedValue: dailyAmount,
                paidValue: 0m,
                pendingValue: dailyAmount,
                closeDate: closeUtc,
                paymentDate: unpaidSentinelUtc,
                processDate: nowUtc
            );

            if (result.IsFailure)
                throw new InvalidOperationException($"No se pudo crear AccumulatedCommission: {result.Error}");

            context.Set<AccumulatedCommission>().Add(result.Value);
            await context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return true; // aplicado por INSERT
        }

        // 5) Existe pero el evento no es más reciente → idempotente (sin cambios)
        await tx.CommitAsync(cancellationToken);
        return false;
    }
}