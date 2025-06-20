using Products.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Application.Objectives.Services;
using Products.Domain.Alternatives;
using Products.Domain.Banks;
using Products.Domain.Commercials;
using Products.Domain.Objectives;
using Products.Domain.Offices;
using Products.Domain.Plans;
using Products.Domain.Portfolios;
using Products.Infrastructure.Alternatives;
using Products.Infrastructure.Banks;
using Products.Infrastructure.Commercials;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.Database;
using Products.Infrastructure.External.Affiliates;
using Products.Infrastructure.Objectives;
using Products.Infrastructure.Offices;
using Products.Infrastructure.Plans;
using Products.Infrastructure.Portfolios;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Products.IntegrationEvents.ContributionValidation;
using Products.IntegrationEvents.PortfolioValidation;

namespace Products.Infrastructure;

public static class ProductsModule
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<ProductsModuleMarker>(typeof(ProductsModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("ProductsDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<ProductsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Products)
                );
        });

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IAlternativeRepository, AlternativeRepository>();
        services.AddScoped<IObjectiveRepository, ObjectiveRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<ICommercialRepository, CommercialRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<ProductsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<ProductsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<ProductsModuleMarker>, ErrorCatalog<ProductsModuleMarker>>();
        
        services.AddScoped<IAffiliateLocator, AffiliateLocator>();
        services.AddScoped<IObjectiveReader, ObjectiveReader>();
        services.AddScoped<IGetObjectivesRules, GetObjectivesRules>();
        services.AddScoped<ContributionValidationConsumer>();
        services.AddTransient<PortfolioValidationConsumer>();


        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductsDbContext>());
    }
}