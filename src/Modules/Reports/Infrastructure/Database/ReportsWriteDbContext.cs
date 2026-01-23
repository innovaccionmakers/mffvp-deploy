using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Reports.Application.Abstractions.Data;
using Reports.Infrastructure.LoadingInfo.Balances;
using Reports.Infrastructure.LoadingInfo.Closing;
using Reports.Infrastructure.LoadingInfo.People;
using Reports.Infrastructure.LoadingInfo.Products;
using Reports.Infrastructure.LoadingExecutions;
using System.Data;

namespace Reports.Infrastructure.Database;

public sealed class ReportsWriteDbContext(DbContextOptions<ReportsWriteDbContext> options)
    : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Reports);
        modelBuilder.ApplyConfiguration(new PeopleSheetConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSheetConfiguration());
        modelBuilder.ApplyConfiguration(new ClosingSheetConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceSheetConfiguration());
        modelBuilder.ApplyConfiguration(new EtlExecutionConfiguration());
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }

    public IDbConnection GetDbConnection()
    {
        return Database.GetDbConnection();
    }
}