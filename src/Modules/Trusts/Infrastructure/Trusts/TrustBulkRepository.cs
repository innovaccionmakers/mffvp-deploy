using Common.SharedKernel.Application.Constants.Closing;
using Common.SharedKernel.Application.Helpers.Money;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Trusts.Domain.Trusts;
using Trusts.Domain.Trusts.TrustYield;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure.Trusts;

internal sealed class TrustBulkRepository(TrustsDbContext context) : ITrustBulkRepository
{
    private const int BulkBatchSize = ClosingBulkProperties.BulkBatchSize;
    public async Task<ApplyYieldBulkResult> ApplyYieldToBalanceBulkAsync(
        IReadOnlyList<ApplyYieldRow> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows is null || rows.Count == 0)
            return new ApplyYieldBulkResult(0, Array.Empty<long>(), Array.Empty<long>());

        var deltas = rows.GroupBy(r => r.TrustId).ToDictionary(
                            g => g.Key,
                            g => new
                            {
                                YieldAmount = g.Sum(x => x.YieldAmount),
                                YieldRetention = g.Sum(x => x.YieldRetention),
                                ClosingBalance = g.Last().ClosingBalance
                            });

        int totalUpdated = 0;
        var allMissing = new List<long>();
        var allMismatches = new List<long>();

        foreach (var chunkIds in deltas.Keys.Chunk(BulkBatchSize))
        {
            var idSet = chunkIds.ToArray();

            // 1) Obtener los fideicomisos existentes 

            var query = context.Set<Trust>()
                 .Where(t => idSet.Contains(t.TrustId))
                 .AsNoTracking()
                 .TagWith("[TrustBulkRepository_ApplyYieldToBalanceBulk_ReadTrusts]");


            var existingTrusts = await query
             .ToListAsync(cancellationToken);

            var existingById = existingTrusts.ToDictionary(x => x.TrustId, x => x);
            var existingSet = existingById.Keys.ToHashSet();

            // 2) Preparar lista a actualizar luego de validar
            var listToUpdate = new List<Trust>(existingTrusts.Count);

            foreach (var trust in existingTrusts)
            {
                var d = deltas[trust.TrustId];

                var prevBalancePlusYield = MoneyHelper.Round2(trust.TotalBalance + d.YieldAmount);

                var newTotalBalance = prevBalancePlusYield;
                var newEarnings = MoneyHelper.Round2(trust.Earnings + d.YieldAmount);
                var newEarningsWithholding = MoneyHelper.Round2(trust.EarningsWithholding + d.YieldRetention);
                var newAvailable = MoneyHelper.Round2(newTotalBalance - newEarningsWithholding - trust.ContingentWithholding);

                trust.UpdateDetails(
                    trust.AffiliateId,
                    trust.ClientOperationId,
                    trust.CreationDate,
                    trust.ObjectiveId,
                    trust.PortfolioId,
                    newTotalBalance,
                    trust.TotalUnits,            
                    trust.Principal,            
                    newEarnings,
                    trust.TaxCondition,           
                    trust.ContingentWithholding,   
                    newEarningsWithholding,
                    newAvailable,
                    trust.Status        
                );

                listToUpdate.Add(trust);
            }

            if (listToUpdate.Count == 0)
                continue;

            // 3) BulkUpdate 
            var updateConfig = new BulkConfig
            {
                CalculateStats = false,
                TrackingEntities = false,   
                SetOutputIdentity = false,
                UseTempDB = false,
                BatchSize = BulkBatchSize,
                UpdateByProperties = new() { nameof(Trust.TrustId) },
                PropertiesToInclude = new List<string>
                {
                    nameof(Trust.TotalBalance),          // saldo_total
                    nameof(Trust.Earnings),              // rendimiento
                    nameof(Trust.EarningsWithholding),   // retencion_rendimiento
                    nameof(Trust.AvailableAmount)        // disponible
                }
            };

            await context.BulkUpdateAsync(
                listToUpdate,
                updateConfig,
                progress: null,
                cancellationToken: cancellationToken);

            totalUpdated += listToUpdate.Count;
        }

        return new ApplyYieldBulkResult(
            Updated: totalUpdated,
            MissingTrustIds: allMissing,
            ValidationMismatchTrustIds: allMismatches
        );
    }
}
