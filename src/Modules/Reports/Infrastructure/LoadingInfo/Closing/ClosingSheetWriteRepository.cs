using Common.SharedKernel.Application.Constants.Reports;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Reports.Infrastructure.Database;
using DomainClosingSheet = Reports.Domain.LoadingInfo.Closing.ClosingSheet;
using IClosingSheetWriteRepo = Reports.Domain.LoadingInfo.Closing.IClosingSheetWriteRepository;

namespace Reports.Infrastructure.LoadingInfo.Closing;

public sealed class ClosingSheetWriteRepository : IClosingSheetWriteRepo
{
    private const int BulkBatchSize = ReportsBulkProperties.EtlBatchSize;

    private readonly IDbContextFactory<ReportsWriteDbContext> dbContextFactory;

    public ClosingSheetWriteRepository(IDbContextFactory<ReportsWriteDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task DeleteByClosingDateAndPortfolioAsync(DateTime closingDateUtc, int portfolioId, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
             "DELETE FROM informes.sabana_cierre WHERE fecha = @p0 AND portafolio_id = @p1;",
          parameters: new object[] { closingDateUtc, portfolioId },
          cancellationToken: cancellationToken);
    }

    public async Task BulkInsertAsync(IReadOnlyList<DomainClosingSheet> rows, CancellationToken cancellationToken)
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