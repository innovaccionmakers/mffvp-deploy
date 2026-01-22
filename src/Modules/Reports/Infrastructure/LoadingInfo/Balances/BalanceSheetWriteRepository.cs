using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Reports.Infrastructure.Database;
using DomainBalanceSheet = Reports.Domain.LoadingInfo.Balances.BalanceSheet;
using IBalanceSheetWriteRepo = Reports.Domain.LoadingInfo.Balances.IBalanceSheetWriteRepository;
using Common.SharedKernel.Application.Constants.Reports;

namespace Reports.Infrastructure.LoadingInfo.Balances;

public sealed class BalanceSheetWriteRepository : IBalanceSheetWriteRepo
{
    private const int BulkBatchSize = ReportsBulkProperties.EtlBatchSize;

    private readonly IDbContextFactory<ReportsWriteDbContext> dbContextFactory;

    public BalanceSheetWriteRepository(IDbContextFactory<ReportsWriteDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task DeleteByClosingDateAndPortfolioAsync(
        DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM informes.sabana_saldos WHERE fecha_cierre = @p0 AND portafolio_id = @p1;",
        parameters: new object[] { closingDateUtc, portfolioId },
   cancellationToken: cancellationToken);
    }

    public async Task BulkInsertAsync(IReadOnlyList<DomainBalanceSheet> rows, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var bulkConfig = new BulkConfig
        {
            BatchSize = BulkBatchSize,
            SetOutputIdentity = false
        };

        await dbContext.BulkInsertAsync(rows.ToList(), bulkConfig, cancellationToken: cancellationToken);
    }
}
