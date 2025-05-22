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
using People.Application.Abstractions;
using People.Application.Abstractions.Rules;
using People.Domain.ConfigurationParameters;
using People.Infrastructure.ConfigurationParameters;
using People.Infrastructure.RulesEngine;
using People.IntegrationEvents.PersonValidation;

namespace People.Infrastructure;

public static class PeopleModule
{
    public static IServiceCollection AddPeopleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<PeopleModuleMarker>(opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
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
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog, ErrorCatalog>();
        services.AddTransient<PersonValidationConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PeopleDbContext>());
    }
}