using System.Data.Common;
using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Associate.Infrastructure.Activates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Associate.Infrastructure.Database;

public sealed class ActivatesDbContext(DbContextOptions<ActivatesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Activate> Activates { get; set; }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Associate);

        modelBuilder.ApplyConfiguration(new ActivateConfiguration());
    }
}