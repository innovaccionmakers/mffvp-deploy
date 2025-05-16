
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Products.Application.Abstractions.Data;
using Products.Domain.Plans;
using Products.Infrastructure.Plans;
using Products.Domain.Alternatives;
using Products.Domain.ConfigurationParameters;
using Products.Infrastructure.Alternatives;
using Products.Domain.Objectives;
using Products.Infrastructure.Objectives;
using Products.Domain.Portfolios;
using Products.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.Portfolios;

namespace Products.Infrastructure.Database;

public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Plan> Plans { get; set; }
    internal DbSet<Alternative> Alternatives { get; set; }
    internal DbSet<Objective> Objectives { get; set; }
    internal DbSet<Portfolio> Portfolios { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Products);

        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new AlternativeConfiguration());
        modelBuilder.ApplyConfiguration(new ObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new PortfolioConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.DisposeAsync();
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}