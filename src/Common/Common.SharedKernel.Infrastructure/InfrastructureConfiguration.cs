using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Infrastructure.Auth.Policy;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using Common.SharedKernel.Infrastructure.EventBus;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Savorboard.CAP.InMemoryMessageQueue;

using Swashbuckle.AspNetCore.SwaggerUI;

using System.Text;

namespace Common.SharedKernel.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        string databaseConnectionString,
        string capDbConnectionString,
        string databaseConnectionStringSQL
    )
    {

        services.AddHttpContextAccessor();
        services.AddAuthentication("JwtBearer")
            .AddJwtBearer("JwtBearer", options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies[".authToken"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "MakersFunsAuthPOC",
                    ValidAudience = "MakersFunsAuthPOC",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my_super_secret_key_1234567890_ABCDEF"))
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();

        services.AddCap(x =>
        {
            x.UseInMemoryStorage();
            x.UseInMemoryMessageQueue();
            x.UsePostgreSql(capDbConnectionString);
            x.FailedRetryInterval = 5;
            x.FailedRetryCount = 10;
            x.UseDashboard();
        });

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql();

                tracing.AddOtlpExporter();
            });

        services.AddScoped<IDatabaseConnectionStrategy, SqlServerConnectionStrategy>();
        services.AddScoped<IDatabaseConnectionStrategy, YugaByteConnectionStrategy>();
        services.AddScoped<DatabaseConnectionContext>();
        services.AddSingleton<ICapRpcClient, CapRpcClient>();
        services.AddSingleton<CapCallbackSubscriber>();

        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            var provider = app.DescribeApiVersions();

            foreach (var description in provider)
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            options.DocExpansion(DocExpansion.None);
        });

        app.UseCors("AllowSwaggerUI");
        
        return app;
    }
}