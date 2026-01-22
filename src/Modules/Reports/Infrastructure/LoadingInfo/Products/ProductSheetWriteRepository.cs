using Common.SharedKernel.Application.Constants.Reports;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Reports.Infrastructure.Database;
using DomainProductSheet = Reports.Domain.LoadingInfo.Products.ProductSheet;
using IProductSheetWriteRepo = Reports.Domain.LoadingInfo.Products.IProductSheetWriteRepository;

namespace Reports.Infrastructure.LoadingInfo.Products;

public sealed class ProductSheetWriteRepository : IProductSheetWriteRepo
{
    private const int BulkBatchSize = ReportsBulkProperties.EtlBatchSize;

    private readonly IDbContextFactory<ReportsWriteDbContext> dbContextFactory;

    public ProductSheetWriteRepository(IDbContextFactory<ReportsWriteDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
         "TRUNCATE TABLE informes.sabana_productos;",
          cancellationToken);
    }

    public async Task DeleteByFundIdsAsync(
        IReadOnlyCollection<int> fundIds,
        CancellationToken cancellationToken)
    {
        if (fundIds == null || fundIds.Count == 0)
            return;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);


        await dbContext.Set<DomainProductSheet>()
       .Where(p => fundIds.Contains(p.FundId))
       .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task BulkInsertAsync(
        IReadOnlyList<DomainProductSheet> rows,
        CancellationToken cancellationToken)
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
