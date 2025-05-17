using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.Rules;
using Products.Domain.Alternatives;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Domain.Plans;
using Products.Domain.Portfolios;
using Products.Infrastructure.Alternatives;
using Products.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.Database;
using Products.Infrastructure.Objectives;
using Products.Infrastructure.Plans;
using Products.Infrastructure.Portfolios;
using Products.Infrastructure.RulesEngine;

namespace Products.Infrastructure;

public static class ProductsModule
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine(opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("ProductsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Products)
                );
        });

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IAlternativeRepository, AlternativeRepository>();
        services.AddScoped<IObjectiveRepository, ObjectiveRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog, ErrorCatalog>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductsDbContext>());
    }
}