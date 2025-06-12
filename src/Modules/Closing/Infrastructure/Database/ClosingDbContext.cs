using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Closing.Application.Abstractions.Data;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.ProfitLosses;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;

namespace Closing.Infrastructure.Database;

public sealed class ClosingDbContext(DbContextOptions<ClosingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ProfitLossConcept> ProfitLossConcepts { get; set; }
    internal DbSet<ProfitLoss> ProfitLosses { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Closing);

        modelBuilder.ApplyConfiguration(new ProfitLossConceptConfiguration());
        modelBuilder.ApplyConfiguration(new ProfitLossConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Closing));
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}