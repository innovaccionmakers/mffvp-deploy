using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Domain.TreasuryMovements;
using Treasury.Infrastructure.BankAccounts;
using Treasury.Infrastructure.Issuers;
using Treasury.Infrastructure.TreasuryConcepts;
using Treasury.Infrastructure.TreasuryMovements;

namespace Treasury.Infrastructure.Database;

public sealed class TreasuryDbContext(DbContextOptions<TreasuryDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Issuer> Issuers { get; set; }
    internal DbSet<BankAccount> BankAccounts { get; set; }
    internal DbSet<TreasuryConcept> TreasuryConcepts { get; set; }
    internal DbSet<TreasuryMovement> TreasuryMovements { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Treasury);

        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Treasury));
        modelBuilder.ApplyConfiguration(new IssuerConfiguration());
        modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new TreasuryConceptConfiguration());
        modelBuilder.ApplyConfiguration(new TreasuryMovementConfiguration());
    }
}