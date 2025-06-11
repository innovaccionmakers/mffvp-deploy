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
using Common.SharedKernel.Application.Rules;
using People.Domain.ConfigurationParameters;
using People.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.RulesEngine;
using People.IntegrationEvents.ClientValidation;
using People.IntegrationEvents.DocumentTypeValidation;
using People.IntegrationEvents.PersonValidation;

namespace People.Infrastructure;

public static class PeopleModule
{
    public static IServiceCollection AddPeopleModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<PeopleModuleMarker>(typeof(PeopleModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("TrustsDatabase");

        if (env == "DevMakers2")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

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
        services.AddScoped<IErrorCatalog<PeopleModuleMarker>, ErrorCatalog>();
        services.AddTransient<PersonValidationConsumer>();
        services.AddTransient<ClientValidationConsumer>();
        services.AddTransient<DocumentTypeValidationConsumer>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PeopleDbContext>());
    }
}