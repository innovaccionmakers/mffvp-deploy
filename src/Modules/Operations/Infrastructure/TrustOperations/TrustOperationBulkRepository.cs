
using Common.SharedKernel.Application.Constants.Closing;
using EFCore.BulkExtensions;
using Operations.Domain.TrustOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.TrustOperations;

internal sealed class TrustOperationBulkRepository(OperationsDbContext context)
    : ITrustOperationBulkRepository
{

    public async Task<UpsertBulkResult> UpsertBulkAsync(
        int portfolioId,
        IReadOnlyList<TrustYieldOpRowForBulk> trustYieldOperations,
        CancellationToken cancellationToken)
    {
        if (trustYieldOperations.Count == 0)
            return new UpsertBulkResult(0, 0, Array.Empty<long>());

        var nowUtc = DateTime.UtcNow;

        var entities = new List<TrustOperation>(trustYieldOperations.Count);
        foreach (var r in trustYieldOperations)
        {
            var created = TrustOperation.Create(
                clientOperationId: r.ClientOperationId,
                trustId: r.TrustId,
                amount: r.Amount,
                operationTypeId: r.OperationTypeId,
                portfolioId: portfolioId,
                registrationDate: nowUtc,
                processDate: r.ProcessDateUtc.Date,
                applicationDate: nowUtc
            );

            entities.Add(created.Value);
        }

        var bulk = new BulkConfig
        {
            BatchSize = ClosingBulkProperties.BulkBatchSize,
            CalculateStats = true,
            TrackingEntities = false,
            SetOutputIdentity = false,

            // Orden del constraint
            UpdateByProperties = new List<string>
            {
                nameof(TrustOperation.PortfolioId),
                nameof(TrustOperation.TrustId),
                nameof(TrustOperation.ProcessDate),
                nameof(TrustOperation.OperationTypeId)
            },


            PropertiesToIncludeOnUpdate = new List<string>
            {
                nameof(TrustOperation.Amount),
                nameof(TrustOperation.ApplicationDate)
            },

        };

        await context.BulkInsertOrUpdateAsync(entities, bulk, cancellationToken: cancellationToken);

        var inserted = bulk.StatsInfo?.StatsNumberInserted ?? 0;
        var updated = bulk.StatsInfo?.StatsNumberUpdated ?? 0;

        var changed = (inserted + updated) > 0
            ? entities.Select(e => e.TrustId).Distinct().ToArray()
            : Array.Empty<long>();

        return new UpsertBulkResult(inserted, updated, changed);
    }
}
