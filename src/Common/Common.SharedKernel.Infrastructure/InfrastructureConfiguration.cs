using Asp.Versioning;

using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain.Aws;
using Common.SharedKernel.Infrastructure.Caching;
using Common.SharedKernel.Infrastructure.Caching.Closing;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using Common.SharedKernel.Infrastructure.EventBus;
using Common.SharedKernel.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Savorboard.CAP.InMemoryMessageQueue;

using Swashbuckle.AspNetCore.SwaggerUI;

using System.Text;

namespace Common.SharedKernel.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
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
            x.UseStorageLock = true;
            x.FailedRetryInterval = 3;
            x.FailedRetryCount = 3;
            x.UseDashboard();

            x.FailedThresholdCallback = failed =>
            {
                //ACA PODRIA GUARDAR EN UNA TABLA DE cap.deadletter, los mensajes que no se pudieron procesar
                // Aquí puedes loguear o alertar
                Console.WriteLine($"Mensaje movido a dead-letter después de {x.FailedRetryCount} intentos. Id: {failed.Message.Value}");
            };
        }).AddSubscribeFilter<CapsEventsFilter>();


        services.AddEndpointsApiExplorer();

        services
            .AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });

        var corsPolicyName = configuration.GetValue<string>("Cors:PolicyName") ?? "FvpCorsPolicy";
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddRedisCache(configuration);

        services.AddScoped<IDatabaseConnectionStrategy, SqlServerConnectionStrategy>();
        services.AddScoped<IDatabaseConnectionStrategy, YugaByteConnectionStrategy>();
        services.AddScoped<DatabaseConnectionContext>();
        services.AddInMemoryRpc();
        services.AddScoped<IEventBus, EventBus.EventBus>();

        services.AddSingleton<IClosingExecutionSerializer, JsonClosingExecutionSerializer>();
        services.AddScoped<IClosingExecutionStore>(sp =>
        {
            var cache = sp.GetRequiredService<IDistributedCache>();
            var serializer = sp.GetRequiredService<IClosingExecutionSerializer>();
            return new DistributedClosingExecutionStore(cache, serializer);
        });

        services.AddNotificationCenter(configuration);
        services.AddFileStorageService(configuration);

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

        var corsPolicyName = app.Configuration.GetValue<string>("Cors:PolicyName") ?? "FvpCorsPolicy";
        app.UseCors(corsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

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