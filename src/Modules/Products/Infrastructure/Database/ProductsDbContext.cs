using Microsoft.EntityFrameworkCore;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore.Storage;
using Products.Application.Abstractions.Data;
using Products.Domain.AccumulatedCommissions;
using Products.Domain.AlternativePortfolios;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.Commissions;
using Products.Domain.Objectives;
using Products.Domain.Offices;
using Products.Domain.PensionFunds;
using Products.Domain.PlanFunds;
using Products.Domain.Plans;
using Products.Domain.Portfolios;
using Products.Domain.TechnicalSheets;
using Products.Infrastructure.AccumulatedCommissions;
using Products.Infrastructure.AlternativePortfolios;
using Products.Infrastructure.Alternatives;
using Products.Infrastructure.Commercials;
using Products.Infrastructure.Commissions;
using Products.Infrastructure.Objectives;
using Products.Infrastructure.Offices;
using Products.Infrastructure.PensionFunds;
using Products.Infrastructure.PlanFunds;
using Products.Infrastructure.Plans;
using Products.Infrastructure.Portfolios;
using Products.Infrastructure.TechnicalSheets;
using System.Data.Common;

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
    internal DbSet<Office> Offices { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<PlanFund> PlanFunds { get; set; }
    internal DbSet<PensionFund> PensionFunds { get; set; }
    internal DbSet<Commission> Commissions { get; set; }
    internal DbSet<AccumulatedCommission> AccumulatedCommissions { get; set; }
    internal DbSet<TechnicalSheet> TechnicalSheets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Products);

        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new AlternativeConfiguration());
        modelBuilder.ApplyConfiguration(new PortfolioConfiguration());
        modelBuilder.ApplyConfiguration(new AlternativePortfolioConfiguration());
        modelBuilder.ApplyConfiguration(new ObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new CommercialConfiguration());
        modelBuilder.ApplyConfiguration(new OfficeConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Products));
        modelBuilder.ApplyConfiguration(new PlanFundConfiguration());
        modelBuilder.ApplyConfiguration(new PensionFundConfiguration());
        modelBuilder.ApplyConfiguration(new CommissionConfiguration());
        modelBuilder.ApplyConfiguration(new AccumulatedCommissionConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalSheetConfiguration());
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }
}