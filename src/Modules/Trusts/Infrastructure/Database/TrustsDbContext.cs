using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.ConfigurationParameters;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.ConfigurationParameters;
using Trusts.Infrastructure.Trusts;

namespace Trusts.Infrastructure.Database;

public sealed class TrustsDbContext(DbContextOptions<TrustsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Trust> Trusts { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Trusts);

        modelBuilder.ApplyConfiguration(new TrustConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
    }
}