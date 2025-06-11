using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using People.Application.Abstractions.Data;
using People.Domain.ConfigurationParameters;
using People.Domain.People;
using People.Infrastructure.People;
using People.Domain.Countries;
using People.Infrastructure.Countries;
using People.Domain.EconomicActivities;
using People.Domain.Municipalities;
using People.Infrastructure.ConfigurationParameters;
using People.Infrastructure.EconomicActivities;
using People.Infrastructure.Municipalities;

namespace People.Infrastructure.Database;

public sealed class PeopleDbContext(DbContextOptions<PeopleDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Person> People { get; set; }
    internal DbSet<Country> Countries { get; set; }
    internal DbSet<EconomicActivity> EconomicActivities { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<Municipality> Municipalities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.People);

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