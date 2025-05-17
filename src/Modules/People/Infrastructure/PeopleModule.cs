using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using People.Application.Abstractions.Data;
using People.Domain.People;
using People.Infrastructure.People;
using People.Domain.Countries;
using People.Infrastructure.Countries;
using People.Domain.EconomicActivities;
using People.Infrastructure.EconomicActivities;
using People.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;

namespace People.Infrastructure;

public static class PeopleModule
{
    public static IServiceCollection AddPeopleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PeopleDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("PeopleDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.People)
                );
        });

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IEconomicActivityRepository, EconomicActivityRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PeopleDbContext>());
    }
}