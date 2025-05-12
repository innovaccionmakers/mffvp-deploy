using System.Data.Common;
using Activations.Application.Abstractions.Data;
using Activations.Domain.Affiliates;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Infrastructure.Affiliates;
using Activations.Infrastructure.MeetsPensionRequirements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Activations.Infrastructure.Database;

public sealed class ActivationsDbContext(DbContextOptions<ActivationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Affiliate> Affiliates { get; set; }
    internal DbSet<MeetsPensionRequirement> MeetsPensionRequirements { get; set; }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Activations);

        modelBuilder.ApplyConfiguration(new AffiliateConfiguration());
        modelBuilder.ApplyConfiguration(new MeetsPensionRequirementConfiguration());
    }
}