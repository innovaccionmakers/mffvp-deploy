using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Products.Application.Abstractions.Data;
using Products.Domain.Plans;
using Products.Infrastructure.Plans;
using Products.Domain.Alternatives;
using Products.Infrastructure.Alternatives;
using Products.Domain.Portfolios;
using Products.Infrastructure.Portfolios;
using Products.Domain.AlternativePortfolios;
using Products.Infrastructure.AlternativePortfolios;
using Products.Domain.Objectives;
using Products.Infrastructure.Objectives;
using Products.Domain.Commercials;
using Products.Infrastructure.Commercials;
using Products.Domain.Cities;
using Products.Domain.ConfigurationParameters;
using Products.Infrastructure.Cities;
using Products.Domain.Offices;
using Products.Domain.PensionFunds;
using Products.Domain.PlanFunds;
using Products.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.Offices;
using Products.Infrastructure.PensionFunds;
using Products.Infrastructure.PlanFunds;

namespace Products.Infrastructure.Database;

public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Plan> Plans { get; set; }
    internal DbSet<Alternative> Alternatives { get; set; }
    internal DbSet<Portfolio> Portfolios { get; set; }
    internal DbSet<AlternativePortfolio> AlternativePortfolios { get; set; }
    internal DbSet<Objective> Objectives { get; set; }
    internal DbSet<Commercial> Commercials { get; set; }
    internal DbSet<City> Cities { get; set; }
    internal DbSet<Office> Offices { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<PlanFund> PlanFunds { get; set; }
    internal DbSet<PensionFund> PensionFunds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Products);

        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new AlternativeConfiguration());
        modelBuilder.ApplyConfiguration(new PortfolioConfiguration());
        modelBuilder.ApplyConfiguration(new AlternativePortfolioConfiguration());
        modelBuilder.ApplyConfiguration(new ObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new CommercialConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new OfficeConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
        modelBuilder.ApplyConfiguration(new PlanFundConfiguration());
        modelBuilder.ApplyConfiguration(new PensionFundConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}