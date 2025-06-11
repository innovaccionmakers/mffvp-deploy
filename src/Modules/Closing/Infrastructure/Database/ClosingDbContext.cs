using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Closing.Application.Abstractions.Data;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.ProfitLosses;

namespace Closing.Infrastructure.Database;

public sealed class ClosingDbContext(DbContextOptions<ClosingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ProfitLossConcept> ProfitLossConcepts { get; set; }
    internal DbSet<ProfitLoss> ProfitLosses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Closing);

        modelBuilder.ApplyConfiguration(new ProfitLossConceptConfiguration());
        modelBuilder.ApplyConfiguration(new ProfitLossConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}