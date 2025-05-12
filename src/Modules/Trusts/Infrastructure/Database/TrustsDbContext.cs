using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.InputInfos;
using Trusts.Domain.TrustOperations;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.CustomerDeals;
using Trusts.Infrastructure.InputInfos;
using Trusts.Infrastructure.TrustOperations;
using Trusts.Infrastructure.Trusts;

namespace Trusts.Infrastructure.Database;

public sealed class TrustsDbContext(DbContextOptions<TrustsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Trust> Trusts { get; set; }
    internal DbSet<CustomerDeal> CustomerDeals { get; set; }
    internal DbSet<TrustOperation> TrustOperations { get; set; }
    internal DbSet<InputInfo> InputInfos { get; set; }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Trusts);

        modelBuilder.ApplyConfiguration(new TrustConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerDealConfiguration());
        modelBuilder.ApplyConfiguration(new TrustOperationConfiguration());
        modelBuilder.ApplyConfiguration(new InputInfoConfiguration());
    }
}