using Accounting.Application.Abstractions.Data;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Concepts;
using Accounting.Domain.Consecutives;
using Accounting.Domain.PassiveTransactions;
using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.AccountingAssistants;
using Accounting.Infrastructure.Concepts;
using Accounting.Infrastructure.Consecutives;
using Accounting.Infrastructure.PassiveTransactions;
using Accounting.Infrastructure.Treasuries;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Accounting.Infrastructure.Database;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : DbContext(options), IUnitOfWork
{

    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<Domain.Treasuries.Treasury> Treasuries { get; set; }
    internal DbSet<PassiveTransaction> PassiveTransactions { get; set; }
    internal DbSet<Concept> Concepts { get; set; }
    internal DbSet<AccountingAssistant> AccountingAssistants { get; set; }
    internal DbSet<Consecutive> Consecutives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounting);

        modelBuilder.ApplyConfiguration(new TreasuryConfiguration());
        modelBuilder.ApplyConfiguration(new PassiveTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new ConceptConfiguration());
        modelBuilder.ApplyConfiguration(new AccountingAssistantConfiguration());
        modelBuilder.ApplyConfiguration(new ConsecutiveConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Accounting));

    }
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
