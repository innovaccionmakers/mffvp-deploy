using System.Data.Common;
using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Associate.Domain.PensionRequirements;
using Associate.Infrastructure.Activates;
using Associate.Infrastructure.PensionRequirements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Associate.Infrastructure.Database;

public sealed class AssociateDbContext(DbContextOptions<AssociateDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Activate> Activates { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<PensionRequirement> PensionRequirements { get; set; }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Associate);

        modelBuilder.ApplyConfiguration(new ActivateConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Associate));
        modelBuilder.ApplyConfiguration(new PensionRequirementConfiguration());
    }
}