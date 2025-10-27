using Closing.Domain.TrustYields;
using Closing.Infrastructure.Database;
using Common.SharedKernel.Application.Constants.Closing;
using EFCore.BulkExtensions;

namespace Closing.Infrastructure.TrustYields;

internal sealed class TrustYieldBulkRepository(ClosingDbContext context) : ITrustYieldBulkRepository
{
    private const int BulkBatchSize = ClosingBulkProperties.BulkBatchSize;

    public async Task BulkUpdateAsync(
      IReadOnlyCollection<TrustYieldUpdateRow> trustYieldRow,
      CancellationToken cancellationToken)
    {
        if (trustYieldRow.Count == 0) return;

        var listToUpdate = new List<TrustYield>(trustYieldRow.Count);
        listToUpdate.Capacity = trustYieldRow.Count;

        foreach (var row in trustYieldRow)
        {

            var entity = TrustYield.Create(
                trustId: row.TrustId,
                portfolioId: row.PortfolioId,
                closingDate: row.ClosingDateUtc,
                participation: row.Participation,
                units: row.Units,
                yieldAmount: row.YieldAmount,
                preClosingBalance: default,     
                closingBalance: row.ClosingBalance,
                income: row.Income,
                expenses: row.Expenses,
                commissions: row.Commissions,
                cost: row.Cost,
                capital: default,                
                processDate: row.ProcessDateUtc,
                contingentRetention: default,     
                yieldRetention: row.YieldRetention
            ).Value;

            listToUpdate.Add(entity);
        }

        var bulkConfig = new BulkConfig
        {
            BatchSize = BulkBatchSize,
            TrackingEntities = false,
            UseTempDB = false,
            CalculateStats = false,
            SetOutputIdentity = false,

            UpdateByProperties = new List<string>
            {
                nameof(TrustYield.PortfolioId),
                nameof(TrustYield.TrustId),
                nameof(TrustYield.ClosingDate)
            },
            PropertiesToInclude = new List<string>
            {
                nameof(TrustYield.Participation),
                nameof(TrustYield.Units),
                nameof(TrustYield.YieldAmount),
                nameof(TrustYield.Income),
                nameof(TrustYield.Expenses),
                nameof(TrustYield.Commissions),
                nameof(TrustYield.Cost),
                nameof(TrustYield.ClosingBalance),
                nameof(TrustYield.YieldRetention),
                nameof(TrustYield.ProcessDate)
            }
        };

        await context.BulkUpdateAsync(listToUpdate, bulkConfig, progress: null, cancellationToken: cancellationToken);
    }
}
