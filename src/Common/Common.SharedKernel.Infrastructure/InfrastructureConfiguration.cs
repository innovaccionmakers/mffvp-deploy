using Common.SharedKernel.Application.Closing;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Infrastructure.Caching;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using Common.SharedKernel.Infrastructure.EventBus;

using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        string databaseConnectionStringSQL,
        string appSettingsSecret
    )
    {
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettingsSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    LifetimeValidator = CustomLifetimeValidator,
                    RequireExpirationTime = true
                };
            });

        services.AddAuthorization();

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
        services.AddScoped<IEventBus, Common.SharedKernel.Infrastructure.EventBus.EventBus>();
        services.AddScoped<IClosingExecutionStore, RedisClosingExecutionStore>();

        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            var provider = app.DescribeApiVersions();

            foreach (var description in provider)
            {
                options.SwaggerEndpoint(
                    $"/fiduciaria/fvp/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }

            options.DocExpansion(DocExpansion.None);
            options.RoutePrefix = "fiduciaria/fvp/swagger";
        });

        app.UseCors("AllowSwaggerUI");

        return app;
    }

    private static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
    {
        if (expires != null)
        {
            return expires > DateTime.UtcNow;
        }
        return false;
    }
}