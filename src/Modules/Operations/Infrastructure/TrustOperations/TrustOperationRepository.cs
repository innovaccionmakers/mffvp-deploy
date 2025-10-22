using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Operations.Domain.TrustOperations;
using Operations.Infrastructure.Database;
namespace Operations.Infrastructure.TrustOperations;

internal sealed class TrustOperationRepository(OperationsDbContext context)
    : ITrustOperationRepository
{
    public async Task AddAsync(TrustOperation operation, CancellationToken cancellationToken)
    {
        await context.TrustOperations.AddAsync(operation, cancellationToken);
    }

    public async Task<TrustOperation?> GetForUpdateByPortfolioTrustAndDateAsync(
      int portfolioId,
      long trustId,
      DateTime closingDate,
      CancellationToken cancellationToken)
    {
        return await context.TrustOperations
            .FirstOrDefaultAsync(
                op => op.PortfolioId == portfolioId
                     && op.TrustId == trustId
                   && op.ProcessDate.Date == closingDate.Date,
                cancellationToken);
    }

    public async Task<TrustOperation?> GetByPortfolioAndTrustAsync(
      int portfolioId,
      long trustId,
      DateTime closingDate,
      CancellationToken cancellationToken)
    {
        return await context.TrustOperations.AsNoTracking()
            .FirstOrDefaultAsync(
                op => op.PortfolioId == portfolioId
                   && op.TrustId == trustId
                   && op.ProcessDate.Date == closingDate.Date,
                cancellationToken);
    }

    public void Update(TrustOperation operation)
    {
        context.TrustOperations.Update(operation);
    }

    public async Task<bool> UpsertAsync(
    int portfolioId,
    long trustId,
    DateTime processDate,
    decimal amount,
    long operationTypeId,
    long? clientOperationId,
    CancellationToken cancellationToken)
    {
        var processDateOnly = processDate.Date;

        var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO operaciones.operaciones_fideicomiso
                (operaciones_clientes_id, fideicomiso_id, valor, tipo_operaciones_id,
                 portafolio_id, fecha_radicacion, fecha_proceso, fecha_aplicacion)
            VALUES
                ({clientOperationId}, {trustId}, {amount}, {operationTypeId},
                 {portfolioId}, NOW(), {processDateOnly}, NOW())
            ON CONFLICT (fideicomiso_id, tipo_operaciones_id, portafolio_id, fecha_proceso )
            DO UPDATE SET
                valor = EXCLUDED.valor,
                fecha_aplicacion = NOW()
            WHERE (operaciones.operaciones_fideicomiso.valor)
                  IS DISTINCT FROM (EXCLUDED.valor);
        ", cancellationToken);


        return rowsAffected > 0;
    }

   
}