using Common.SharedKernel.Application.Constants.Reports;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Reports.Infrastructure.Database;
using DomainPeopleSheet = Reports.Domain.LoadingInfo.People.PeopleSheet;
using IPeopleSheetWriteRepo = Reports.Domain.LoadingInfo.People.IPeopleSheetWriteRepository;

namespace Reports.Infrastructure.LoadingInfo.People;

public sealed class PeopleSheetWriteRepository : IPeopleSheetWriteRepo
{
    private readonly IDbContextFactory<ReportsWriteDbContext> dbContextFactory;

    public PeopleSheetWriteRepository(IDbContextFactory<ReportsWriteDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
             "TRUNCATE TABLE informes.sabana_personas_afiliados;",
            cancellationToken);
    }

    public async Task DeleteByMemberIdsAsync(
          IReadOnlyCollection<long> memberIds,
          CancellationToken cancellationToken)
    {
        if (memberIds == null || memberIds.Count == 0)
            return;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Set<DomainPeopleSheet>()
          .Where(p => memberIds.Contains(p.AffiliateId))
          .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task BulkInsertAsync(
    IReadOnlyList<DomainPeopleSheet> rows,
    CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var bulkConfig = new BulkConfig
        {
            BatchSize = ReportsBulkProperties.EtlBatchSize,
            SetOutputIdentity = false
        };

        await dbContext.BulkInsertAsync(rows.ToList(), bulkConfig, cancellationToken: cancellationToken);
    }
}
