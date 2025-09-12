using Closing.Application.Abstractions.Data;
using Closing.Domain.ClientOperations;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Domain.TrustYields;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Infrastructure.ClientOperations;
using Closing.Infrastructure.PortfolioValuations;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.ProfitLosses;
using Closing.Infrastructure.TrustYields;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Closing.Infrastructure.Database;

public sealed class ClosingDbContext(DbContextOptions<ClosingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ProfitLossConcept> ProfitLossConcepts { get; set; }
    internal DbSet<ProfitLoss> ProfitLosses { get; set; }
    internal DbSet<Yield> Yields { get; set; }
    internal DbSet<YieldDetail> YieldDetails { get; set; }
    internal DbSet<TrustYield> TrustYields { get; set; }
    internal DbSet<ClientOperation> ClientOperations { get; set; }
    internal DbSet<PortfolioValuation> PortfolioValuations { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Closing);

        modelBuilder.ApplyConfiguration(new ProfitLossConceptConfiguration());
        modelBuilder.ApplyConfiguration(new ProfitLossConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.Yields.YieldConfiguration());
        modelBuilder.ApplyConfiguration(new Infrastructure.YieldDetails.YieldDetailConfiguration());
        modelBuilder.ApplyConfiguration(new TrustYieldConfiguration());
        modelBuilder.ApplyConfiguration(new ClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new PortfolioValuationConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Closing));
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