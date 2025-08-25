using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Customers.Application.Abstractions.Data;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Customers.Domain.People;
using Customers.Infrastructure.People;
using Customers.Domain.Countries;
using Customers.Domain.Departments;
using Customers.Infrastructure.Countries;
using Customers.Domain.EconomicActivities;
using Customers.Domain.Municipalities;
using Customers.Infrastructure.ConfigurationParameters;
using Customers.Infrastructure.Departments;
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
    internal DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Customers);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new EconomicActivityConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
        modelBuilder.ApplyConfiguration(new MunicipalityConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
            await Database.CurrentTransaction.DisposeAsync();

        return await Database.BeginTransactionAsync(cancellationToken);
    }
}