using Application.People;
using Application.People.GetPerson;

using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.RulesEngine;

using Customers.Application.Abstractions;
using Customers.Application.Abstractions.Data;
using Customers.Domain.ConfigurationParameters;
using Customers.Domain.Countries;
using Customers.Domain.Departments;
using Customers.Domain.EconomicActivities;
using Customers.Domain.Municipalities;
using Customers.Domain.People;
using Customers.Infrastructure.ConfigurationParameters;
using Customers.Infrastructure.Countries;
using Customers.Infrastructure.Database;
using Customers.Infrastructure.EconomicActivities;
using Customers.Infrastructure.People;
using Customers.IntegrationEvents.ClientValidation;
using Customers.IntegrationEvents.PersonValidation;
using Customers.Presentation.GraphQL;
using Customers.Presentation.MinimalApis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Infrastructure;

public class CustomersModule : IModuleConfiguration
{
    public string ModuleName => "Customers";
    public string RoutePrefix => "api/customers";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<CustomersModuleMarker>(typeof(CustomersModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("CustomersDatabase");

        if (env != "Development")
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
        services.AddScoped<ICustomersExperienceQueries, CustomersExperienceQueries>();

        services.AddScoped<IConfigurationParameterLookupRepository<CustomersModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<CustomersModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<CustomersModuleMarker>, ErrorCatalog<CustomersModuleMarker>>();
        services.AddTransient<PersonValidationConsumer>();
        services.AddTransient<ClientValidationConsumer>();
        services.AddTransient<PersonCommandHandlerValidation>();
        services.AddTransient<GetPersonQueryHandlerValidation>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomersDbContext>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        if (app is WebApplication webApp)
        {
            webApp.MapCustomersBusinessEndpoints();
        }
    }
}