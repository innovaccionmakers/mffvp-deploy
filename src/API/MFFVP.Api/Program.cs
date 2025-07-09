using Asp.Versioning;

using Common.SharedKernel.Application;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Extensions;
using Common.SharedKernel.Infrastructure.Validation;
using Common.SharedKernel.Infrastructure.Caching;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;

using FluentValidation;

using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;

using Microsoft.AspNetCore.Authorization;

using Serilog;

using System.Reflection;

using Trusts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

if (env != "Development")
{
    var secretName = builder.Configuration["AWS:SecretsManager:SecretName"];
    var region = builder.Configuration["AWS:SecretsManager:Region"];
    var response = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();

    using (var tempProvider = builder.Services.BuildServiceProvider())
    {
        var logger = tempProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("ILogger is working: environment is {EnvironmentName}", builder.Environment.EnvironmentName);

        if (string.IsNullOrWhiteSpace(response))
        {
            logger.LogError("Secret fetched from SecretsManager is empty or null.");
        }
        else
        {
            logger.LogInformation("Secret fetched from SecretsManager successfully and assigned to configuration.");
        }
    }

    builder.Configuration["ConnectionStrings:Database"] = response;
    builder.Configuration["ConnectionStrings:CapDatabase"] = response;
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient(typeof(IValidator<>), typeof(TechnicalValidator<>));
builder.Services.AddSingleton(typeof(TechnicalValidationFilter<>));

builder.Services.AddSwaggerGen();

Assembly[] moduleApplicationAssemblies =
[
    Associate.Application.AssemblyReference.Assembly,
    Trusts.Application.AssemblyReference.Assembly,
    Products.Application.AssemblyReference.Assembly,
    Customers.Application.AssemblyReference.Assembly,
    Operations.Application.AssemblyReference.Assembly,
    Closing.Application.AssemblyReference.Assembly,
    Security.Application.AssemblyReference.Assembly,
    Treasury.Application.AssemblyReference.Assembly
];

builder.Services.AddApplication(moduleApplicationAssemblies);

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
var databaseConnectionStringSQL = builder.Configuration.GetConnectionStringOrThrow("SqlServerDatabase");
var capDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("CapDatabase");


builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    databaseConnectionString,
    capDbConnectionString,
    databaseConnectionStringSQL);

builder.Services.AddRedisCache(builder.Configuration);

builder.Configuration.AddModuleConfiguration(["trusts", "associate", "products", "customers", "operations", "closing", "treasury"], env);

builder.Services.AddTrustsModule(builder.Configuration);

var activatesModuleAssembly = Assembly.GetAssembly(typeof(Associate.Infrastructure.ActivatesModule));
if (activatesModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(activatesModuleAssembly, builder.Configuration);
}

var productsModuleAssembly = Assembly.GetAssembly(typeof(Products.Infrastructure.ProductsModule));
if (productsModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(productsModuleAssembly, builder.Configuration);
}

var customersModuleAssembly = Assembly.GetAssembly(typeof(Customers.Infrastructure.CustomersModule));
if (customersModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(customersModuleAssembly, builder.Configuration);
}

var operationsModuleAssembly = Assembly.GetAssembly(typeof(Operations.Infrastructure.OperationsModule));
if (operationsModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(operationsModuleAssembly, builder.Configuration);
}

var closingModuleAssembly = Assembly.GetAssembly(typeof(Closing.Infrastructure.ClosingModule));
if (closingModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(closingModuleAssembly, builder.Configuration);
}

var securityModuleAssembly = Assembly.GetAssembly(typeof(Security.Infrastructure.SecurityModule));
if (securityModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(securityModuleAssembly, builder.Configuration);
}

var treasuryModuleAssembly = Assembly.GetAssembly(typeof(Treasury.Infrastructure.TreasuryModule));
if (treasuryModuleAssembly != null)
{
    builder.Services.AddModulesFromAssembly(treasuryModuleAssembly, builder.Configuration);
}

var bffAssembly = Assembly.GetAssembly(typeof(MFFVP.BFF.ModuleConfiguration));
if (bffAssembly != null)
{
    builder.Services.AddModulesFromAssembly(bffAssembly, builder.Configuration);
}

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(moduleApplicationAssemblies));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI", policy =>
    {
        policy.WithOrigins("https://localhost:7203", "https://localhost:5173", "http://localhost:3000", "https://mffvp-frontend.pages.dev")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services
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

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

var moduleConfigurations = app.Services.GetServices<IModuleConfiguration>();
foreach (var module in moduleConfigurations)
{
    module.Configure(app, app.Environment);
}

app.UsePathBase("/fiduciaria/fvp");

app.UseInfrastructure();

app.UseLogContext();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/",
    () => Results.Ok(new { module = "MFFVP", version = $"v.{Assembly.GetExecutingAssembly().GetName().Version}" }));


//app.MapGet("/api/userinfo", [Authorize(AuthenticationSchemes = "JwtBearer")] (HttpContext context) =>
//{
//    var userId = context.User.FindFirst("sub")?.Value;
//    var userName = context.User.Identity?.Name;
//    return Results.Ok($"Welcome {userName} (ID: {userId}), this is protected data.");
//});

//// Allowed: Cristof (direct permission)
//app.MapGet("/api/secured/users/delete", [Authorize(Policy = "FVP.Users.Users.Delete")] () =>
//{
//    return Results.Ok("Access granted for Users.Delete");
//});

//// Allowed: CristianVillalobos (direct permission)
//app.MapGet("/api/secured/activates/update", [Authorize(Policy = "FVP.Associate.Activates.Update")] () =>
//{
//    return Results.Ok("Access granted for Activates.Update");
//});

//// Allowed: Cristof (direct permission)
//app.MapGet("/api/secured/activates/view", [Authorize(Policy = "FVP.Associate.Activates.View")] () =>
//{
//    return Results.Ok("Access granted for Activates.View");
//});

//// Allowed: Editor role (Cristof, CristianVillalobos)
//app.MapGet("/api/secured/pension-requirements/update", [Authorize(Policy = "FVP.Operations.Contribution.Crear")] () =>
//{
//    return Results.Ok("Access granted for PensionRequirements.Update");
//});

AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.WriteLine("Shutting down...");

Console.WriteLine("Application has reached app.Run()");

app.Run();