using Microsoft.EntityFrameworkCore;
using Products.Domain.AccumulatedCommissions;
using Products.Infrastructure.Database;

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

    /// <summary>
    /// UPSERT SQL-first para productos.comisiones_acumuladas.
    /// - dailyAmount: delta del día que llega en el evento (NO el acumulado).
    /// - Idempotente: sólo actualiza si el closingDateEvent es posterior al almacenado.
    /// - Requiere UNIQUE (portfolio_id, comisiones_id).
    /// </summary>
    public async Task<bool> UpsertAsync(
        int portfolioId,
        int commissionId,
        DateTime closingDateEventUtc,
        decimal dailyAmount,
        CancellationToken cancellationToken)
    {

        var closeTsUtc = closingDateEventUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(closingDateEventUtc, DateTimeKind.Utc)
            : closingDateEventUtc.ToUniversalTime();
        

        var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO productos.comisiones_acumuladas
                (portfolio_id, comisiones_id, valor_acumulado, valor_pagado, valor_pendiente,
                 fecha_cierre, fecha_pago, fecha_proceso)
            VALUES
                ({portfolioId}, {commissionId}, {dailyAmount}::numeric(19,2), 0::numeric(19,2),
                 GREATEST({dailyAmount}::numeric(19,2) - 0::numeric(19,2), 0::numeric(19,2)),
                 {closeTsUtc}, TIMESTAMPTZ '-infinity', NOW())
            ON CONFLICT (portfolio_id, comisiones_id)
            DO UPDATE SET
                valor_acumulado = CASE
                    WHEN productos.comisiones_acumuladas.fecha_cierre::date < EXCLUDED.fecha_cierre::date
                         THEN round(productos.comisiones_acumuladas.valor_acumulado + EXCLUDED.valor_acumulado, 2)
                    ELSE productos.comisiones_acumuladas.valor_acumulado
                END,
                fecha_cierre   = GREATEST(productos.comisiones_acumuladas.fecha_cierre, EXCLUDED.fecha_cierre),
                fecha_proceso  = NOW(),
                valor_pendiente = GREATEST(
                    CASE
                        WHEN productos.comisiones_acumuladas.fecha_cierre::date < EXCLUDED.fecha_cierre::date
                             THEN round(productos.comisiones_acumuladas.valor_acumulado + EXCLUDED.valor_acumulado, 2) - productos.comisiones_acumuladas.valor_pagado
                        ELSE productos.comisiones_acumuladas.valor_acumulado - productos.comisiones_acumuladas.valor_pagado
                    END,
                    0::numeric(19,2)
                )
            WHERE productos.comisiones_acumuladas.fecha_cierre::date < EXCLUDED.fecha_cierre::date;
        ", cancellationToken);
    
        return rowsAffected > 0;
    }
}