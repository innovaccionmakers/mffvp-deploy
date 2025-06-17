using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Customers.Application.Abstractions.Data;
using Customers.Domain.People;
using Customers.Infrastructure.People;
using Customers.Domain.Countries;
using Customers.Infrastructure.Countries;
using Customers.Domain.EconomicActivities;
using Customers.Infrastructure.EconomicActivities;
using Customers.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;
using Customers.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Customers.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Customers.IntegrationEvents.ClientValidation;
using Customers.IntegrationEvents.PersonValidation;
using Customers.Domain.ConfigurationParameters;
using Application.People;
using Customers.Domain.Departments;
using Customers.Domain.Municipalities;
using Application.People.GetPerson;

namespace Customers.Infrastructure;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<CustomersModuleMarker>(typeof(CustomersModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("CustomersDatabase");

        if (env == "DevMakers2")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<CustomersDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Customers)
                );
        });

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEconomicActivityRepository, EconomicActivityRepository>();
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<CustomersModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<CustomersModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<CustomersModuleMarker>, ErrorCatalog<CustomersModuleMarker>>();
        services.AddTransient<PersonValidationConsumer>();
        services.AddTransient<ClientValidationConsumer>();
        services.AddTransient<PersonCommandHandlerValidation>();
        services.AddTransient<GetPersonQueryHandlerValidation>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomersDbContext>());
    }
}