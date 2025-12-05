using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Database.Interceptors;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.External.Closing;
using Products.Application.Abstractions.External.Trusts;
using Products.Application.Abstractions.Services.AdditionalInformation;
using Products.Application.Abstractions.Services.External;
using Products.Application.Abstractions.Services.Objectives;
using Products.Application.Abstractions.Services.Rules;
using Products.Application.Objectives.Services;
using Products.Domain.AccumulatedCommissions;
using Products.Domain.Administrators;
using Products.Domain.Alternatives;
using Products.Domain.Commercials;
using Products.Domain.Commissions;
using Products.Domain.ConfigurationParameters;
using Products.Domain.Objectives;
using Products.Domain.Offices;
using Products.Domain.PensionFunds;
using Products.Domain.PlanFunds;
using Products.Domain.Plans;
using Products.Domain.Portfolios;
using Products.Domain.TechnicalSheets;
using Products.Infrastructure.AccumulatedCommissions;
using Products.Infrastructure.AdditionalInformation;
using Products.Infrastructure.Administrators;
using Products.Infrastructure.Alternatives;
using Products.Infrastructure.Commercials;
using Products.Infrastructure.Commissions;
using Products.Infrastructure.ConfigurationParameters;
using Products.Infrastructure.Database;
using Products.Infrastructure.External.Affiliates;
using Products.Infrastructure.External.ObjectivesValidation;
using Products.Infrastructure.External.PortfolioValuations;
using Products.Infrastructure.External.Trusts;
using Products.Infrastructure.Objectives;
using Products.Infrastructure.Offices;
using Products.Infrastructure.PensionFunds;
using Products.Infrastructure.PlanFunds;
using Products.Infrastructure.Plans;
using Products.Infrastructure.Portfolios;
using Products.Infrastructure.TechnicalSheets;
using Products.IntegrationEvents.AccumulatedCommissions;
using Products.IntegrationEvents.AdditionalInformation;
using Products.IntegrationEvents.Commission.CommissionsByPortfolio;
using Products.IntegrationEvents.ContributionValidation;
using Products.IntegrationEvents.EntityValidation;
using Products.IntegrationEvents.Portfolio;
using Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;
using Products.IntegrationEvents.Portfolio.GetInfoForClosing;
using Products.IntegrationEvents.Portfolio.GetPortfolioInformation;
using Products.IntegrationEvents.Portfolio.UpdateFromClosing;
using Products.IntegrationEvents.PortfolioValidation;
using Products.IntegrationEvents.TechnicalSheet;
using Products.Integrations.Portfolios.Queries;
using Products.Presentation.GraphQL;
using Products.Presentation.MinimalApis;

namespace Products.Infrastructure;

public class ProductsModule: IModuleConfiguration
{
    public string ModuleName => "Productos";
    public string RoutePrefix => "api/products";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<ProductsModuleMarker>(typeof(ProductsModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

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
                .AddInterceptors(sp.GetRequiredService<PreviousStateSaveChangesInterceptor>())
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Products)
                );
        });

        services.AddScoped<IPreviousStateProvider, PreviousStateProvider>();
        services.AddScoped<PreviousStateSaveChangesInterceptor>();

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IAlternativeRepository, AlternativeRepository>();
        services.AddScoped<IObjectiveRepository, ObjectiveRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPlanFundRepository, PlanFundRepository>();
        services.AddScoped<ICommercialRepository, CommercialRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IPensionFundRepository, PensionFundRepository>();
        services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<ProductsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<ProductsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<ProductsModuleMarker>, ErrorCatalog<ProductsModuleMarker>>();
        services.AddScoped<IProductsExperienceQueries, ProductsExperienceQueries>();
        services.AddScoped<IProductsExperienceMutations, ProductsExperienceMutations>();

        services.AddScoped<IAffiliateLocator, AffiliateLocator>();
        services.AddScoped<IPortfolioValuationLocator, PortfolioValuationLocator>();
        services.AddScoped<ITrustYieldLocator, TrustYieldLocator>();
        services.AddScoped<IObjectivesValidationTrusts, ObjectivesValidationTrusts>();
        services.AddScoped<IObjectiveReader, ObjectiveReader>();
        services.AddScoped<IAdditionalInformationService, AdditionalInformationService>();
        services.AddScoped<IGetObjectivesRules, GetObjectivesRules>();
        services.AddScoped<IRpcHandler<ContributionValidationRequest, ContributionValidationResponse>, ContributionValidationConsumer>();
        services.AddScoped<IRpcHandler<GetPortfolioByIdRequest, GetPortfolioByIdResponse>, GetPortfolioByIdConsumer>();
        services.AddScoped<IRpcHandler<GetHomologateCodeByObjetiveIdRequest, GetHomologateCodeByObjetiveIdResponse>, GetHomologateCodeByObjetiveIdConsumer>();
        services.AddScoped<IRpcHandler<GetPortfolioByHomologatedCodeRequest, GetPortfolioByHomologatedCodeResponse>, GetPortfolioByHomologatedCodeConsumer>();
        services.AddTransient<IRpcHandler<ValidatePortfolioRequest, ValidatePortfolioResponse>, PortfolioValidationConsumer>();
        services.AddTransient<IRpcHandler<GetPortfolioDataRequest, GetPortfolioDataResponse>, PortfolioValidationConsumer>();
        services.AddTransient<IRpcHandler<ValidateEntityRequest, ValidateEntityResponse>, ValidateEntityConsumer>();

        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddTransient<IRpcHandler<CommissionsByPortfolioRequest, CommissionsByPortfolioResponse>, CommissionsByPortfolioConsumer>();
        services.AddTransient<IRpcHandler<GetAdditionalInformationRequest, GetAdditionalInformationResponse>, GetAdditionalInformationConsumer>();
        services.AddTransient<IRpcHandler<GetPortfolioInformationByIdRequest, GetPortfolioInformationByIdResponse>, GetPortfolioInformationByIdConsumer>();
        services.AddTransient<IRpcHandler<AreAllPortfoliosClosedRequest, AreAllPortfoliosClosedResponse>, AreAllPortfoliosClosedConsumer>();
        services.AddTransient<IRpcHandler<GetPortfolioInfoForClosingRequest, GetPortfolioInfoForClosingResponse>, GetPortfolioInfoForClosingConsumer>();

        services.AddScoped<IAccumulatedCommissionRepository, AccumulatedCommissionRepository>();
        services.AddScoped<ITechnicalSheetRepository, TechnicalSheetRepository>();
        services.AddScoped<TechnicalSheetDataBuilderSuscriber>();

        services.AddScoped<IRpcHandler<UpdatePortfolioFromClosingRequest, UpdatePortfolioFromClosingResponse>, UpdatePortfolioFromClosingConsumer>();
        services.AddScoped<IRpcHandler<UpdateAccumulatedCommissionFromClosingRequest, UpdateAccumulatedCommissionFromClosingResponse>, UpdateAccumulatedCommissionFromClosingConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductsDbContext>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapProductsBusinessEndpoints();
        }
    }
}