using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Contributions.Application.Abstractions.Data;
using Contributions.Domain.Trusts;


using Contributions.Domain.ClientOperations;
using Contributions.Domain.TrustOperations;
using Contributions.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;
using Contributions.Infrastructure.RulesEngine;

namespace Contributions.Infrastructure
{
    public static class ContributionsModule
    {
        public static IServiceCollection AddContributionsModule(this IServiceCollection services, IConfiguration configuration)
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
            services.AddDbContext<ContributionsDbContext>((sp, options) =>
            {
                options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                       .UseNpgsql(
                            configuration.GetConnectionString("ContributionsDatabase"),
                            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Contributions)
                       );
            });

            services.AddScoped<ITrustRepository, TrustRepository>();
            services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
            services.AddScoped<ITrustOperationRepository, TrustOperationRepository>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ContributionsDbContext>());
        }
    }
}