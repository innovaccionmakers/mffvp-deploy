using Common.SharedKernel.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Security.Application.Abstractions.Data;
using Security.Domain.RolePermissions;
using Security.Domain.Roles;
using Security.Domain.UserPermissions;
using Security.Domain.UserRoles;
using Security.Domain.Users;
using Security.Infrastructure.Database;
using Security.Infrastructure.RolePermissions;
using Security.Infrastructure.Roles;
using Security.Infrastructure.UserPermissions;
using Security.Infrastructure.UserRoles;
using Security.Infrastructure.Users;

namespace Security.Infrastructure;

public static class OperationsModule
{
    public static IServiceCollection AddSecurityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("OperationsDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<SecurityDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Security)
                );
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SecurityDbContext>());
    }
}
