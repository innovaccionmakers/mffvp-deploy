using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Customers.Application.Abstractions.Data;
using Customers.Domain.ConfigurationParameters;
using Customers.Domain.People;
using Customers.Infrastructure.People;
using Customers.Domain.Countries;
using Customers.Infrastructure.Countries;
using Customers.Domain.EconomicActivities;
using Customers.Domain.Municipalities;
using Customers.Infrastructure.ConfigurationParameters;
using Customers.Infrastructure.EconomicActivities;
using Customers.Infrastructure.Municipalities;

namespace Customers.Infrastructure.Database;

public sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Person> Customers { get; set; }
    internal DbSet<Country> Countries { get; set; }
    internal DbSet<EconomicActivity> EconomicActivities { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<Municipality> Municipalities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Customers);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new EconomicActivityConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
        modelBuilder.ApplyConfiguration(new MunicipalityConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}