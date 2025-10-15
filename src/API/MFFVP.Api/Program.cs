using Common.SharedKernel.Application;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Extensions;
using Common.SharedKernel.Infrastructure.Validation;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;

using FluentValidation;

#if !IS_CI
using Makers.Adp.Telemetry.Models;
using Makers.Adp.Telemetry.ServiceExtensions;
#endif

using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;

using Microsoft.OpenApi.Models;

using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (env != "Development")
{
    #if !IS_CI
            var observabilityOptions = builder.Configuration.GetSection("Observability").Get<ObservabilityOptions>();
        builder.Services.AddObservabilityServiceExtension(options =>
        {
            options.ServiceName = observabilityOptions.ServiceName;
            options.MeterNames = observabilityOptions.MeterNames;
            options.OtlpEndpoint = observabilityOptions.OtlpEndpoint;
            options.EnableConsoleExporter = observabilityOptions.EnableConsoleExporter;
            options.DefaultAttributes = observabilityOptions.DefaultAttributes;

            options.EnableAutoTracingLogging = true;

            options.UseSidecarPattern = true;
            options.SidecarEndpoint = "http://localhost:4317";
            options.EnablePrometheusExporter = false;
            options.EnableAspireExport = true;

            options.AutoTracingAssemblyPatterns = new[]
            {
                    "Makers.Funds.*",            // Todos los assemblies Makers.Funds
                    "Core.Makers.Funds.*",       // Core assemblies (para IErrorOperationsBusiness)
                    "Makers.*.Bussines",         // Capas de negocio
                    "Makers.*.Business",         // Por si usan "Business" sin "s"
                    "Makers.*.Data",             // Capas de datos
                    "*.Application",
                    "*.Application.Contracts",
                    "*.Domain",
                    "*.Infrastructure",
                    "*.IntegrationEvents",
                    "*.Integrations",
                    "*.Presentation",
                    "MFFVP.Api"
            };

            options.AutoTracingServicePatterns = new[]
            {
                    "*Business",                 // IErrorOperationsBusiness
                    "*Bussines",                 // IEscrowBussines, IInconsistencyBussines, IClosingLogBussines
                    "*Service",                  // Servicios generales
                    "*Repository",               // Repositorios
                    "*Handler",                  // Handlers
                    "*Manager",                  // Managers
                    "*Processor"                 // Procesadores
            };

            options.AutoTracingExcludePatterns = new[]
            {
                    "*HealthCheck*",             // Health checks
                    "*Configuration*",           // Configuraciones
                    "*Logger*",                  // Loggers (pueden crear recursi�n)
                    "*.Internal.*"               // Clases internas
            };
        });

        var secretName = builder.Configuration["AWS:SecretsManager:SecretName"];
        var region = builder.Configuration["AWS:SecretsManager:Region"];
        var response = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();

        builder.Configuration["ConnectionStrings:Database"] = response;
        builder.Configuration["ConnectionStrings:CapDatabase"] = response;
    #endif
}


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddTransient(typeof(IValidator<>), typeof(TechnicalValidator<>));
builder.Services.AddSingleton(typeof(TechnicalValidationFilter<>));

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

Assembly[] moduleApplicationAssemblies =
[
    Accounting.Application.AssemblyReference.Assembly,
    Associate.Application.AssemblyReference.Assembly,
    Trusts.Application.AssemblyReference.Assembly,
    Products.Application.AssemblyReference.Assembly,
    Customers.Application.AssemblyReference.Assembly,
    Operations.Application.AssemblyReference.Assembly,
    Closing.Application.AssemblyReference.Assembly,
    Security.Application.AssemblyReference.Assembly,
    Treasury.Application.AssemblyReference.Assembly,
    DataSync.Application.AssemblyReference.Assembly,
    Reports.Application.AssemblyReference.Assembly
];

builder.Services.AddApplication(moduleApplicationAssemblies);

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
var databaseConnectionStringSQL = builder.Configuration.GetConnectionStringOrThrow("SqlServerDatabase");
var capDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("CapDatabase");

var appSettingsSecret = builder.Configuration["CustomSettings:Secret"];

builder.Services.AddInfrastructure(
    builder.Configuration,
    DiagnosticsConfig.ServiceName,
    databaseConnectionString,
    capDbConnectionString,
    databaseConnectionStringSQL,
    appSettingsSecret);

builder.Configuration.AddModuleConfiguration(["trusts", "associate", "products", "customers", "operations", "closing", "treasury", "datasync", "reports", "accounting"], env);

var modules = new List<Type>
{
    typeof(Trusts.Infrastructure.TrustsModule),
    typeof(Associate.Infrastructure.ActivatesModule),
    typeof(Products.Infrastructure.ProductsModule),
    typeof(Customers.Infrastructure.CustomersModule),
    typeof(Operations.Infrastructure.OperationsModule),
    typeof(Closing.Infrastructure.ClosingModule),
    typeof(Security.Infrastructure.SecurityModule),
    typeof(Treasury.Infrastructure.TreasuryModule),
    typeof(DataSync.Infrastructure.DataSyncModule),
    typeof(Reports.Infrastructure.ReportsModule),
    typeof(MFFVP.BFF.ModuleConfiguration),
    typeof(Accounting.Infrastructure.AccountingModule)
};


foreach (var moduleType in modules)
{
    var moduleAssembly = Assembly.GetAssembly(moduleType);
    if (moduleAssembly != null)
    {
        builder.Services.AddModulesFromAssembly(moduleAssembly, builder.Configuration);
    }
}

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(moduleApplicationAssemblies));

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

app.UsePathBase("/fiduciaria/fvp");

if (env != "Development")
{
#if !IS_CI
    app.UseOtelMiddleware();
#endif
}

app.UseInfrastructure();

//app.UseLogContext();

var moduleConfigurations = app.Services.GetServices<IModuleConfiguration>();

foreach (var module in moduleConfigurations)
{
    module.Configure(app, app.Environment);
}

app.MapEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", () =>
{
    var sw = Stopwatch.StartNew();

    var asm = Assembly.GetExecutingAssembly().GetName();
    var name = "Makers.Module.MFFVP";
    var version = asm.Version?.ToString() ?? "0.0.0.0";

    sw.Stop();

    return Results.Ok(new
    {
        status = "Healthy",
        duration = sw.Elapsed.ToString(@"hh\:mm\:ss\.fffffff"),
        name,
        version
    });
});
AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.WriteLine("Shutting down...");

Console.WriteLine("Application has reached app.Run()");

app.Run();