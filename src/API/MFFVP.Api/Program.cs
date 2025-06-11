using Asp.Versioning;

using Associate.Infrastructure;

using Common.SharedKernel.Application;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Validation;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;

using FluentValidation;

using MFFVP.Api.BffWeb.Associate;
using MFFVP.Api.BffWeb.Associate.PensionRequirements;
using MFFVP.Api.BffWeb.Operations;
using MFFVP.Api.BffWeb.People;
using MFFVP.Api.BffWeb.Products;
using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;

using Microsoft.AspNetCore.Authorization;

using Operations.Infrastructure;

using People.Infrastructure;

using Products.Infrastructure;

using Serilog;

using System.Reflection;

using Trusts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
//builder.Configuration
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
//    .AddEnvironmentVariables();

if (env == "DevMakers2")
{
    var secretName = builder.Configuration["AWS:SecretsManager:SecretName"];
    var region = builder.Configuration["AWS:SecretsManager:Region"];
    var response = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();

    builder.Configuration["ConnectionStrings:Database"] = response;
    builder.Configuration["ConnectionStrings:capDbConnectionString"] = response;
}


builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

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
    People.Application.AssemblyReference.Assembly,
    Operations.Application.AssemblyReference.Assembly,
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

builder.Configuration.AddModuleConfiguration(["trusts", "associate", "products", "people", "operations"], env);

builder.Services.AddTrustsModule(builder.Configuration);
builder.Services.AddActivatesModule(builder.Configuration);
builder.Services.AddProductsModule(builder.Configuration);
builder.Services.AddPeopleModule(builder.Configuration);
builder.Services.AddOperationsModule(builder.Configuration);

builder.Services.AddBffActivatesServices();
builder.Services.AddBffProductsServices();
builder.Services.AddBffPeopleServices();
builder.Services.AddBffOperationsServices();

builder.Services.AddEndpoints(typeof(AssociateEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(ProductsEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(PeopleEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(OperationsEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(PensionRequirementsEndpoints).Assembly);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(moduleApplicationAssemblies));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI", policy =>
    {
        policy.WithOrigins("https://localhost:7203", "https://localhost:5173")
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

app.UsePathBase("/fiduciaria/fvp");

app.UseInfrastructure();

app.UseLogContext();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.MapGet("/",
    () => Results.Ok(new { module = "MFFVP", version = $"v.{Assembly.GetExecutingAssembly().GetName().Version}" }));

app.MapGet("/api/userinfo", [Authorize(AuthenticationSchemes = "JwtBearer")] (HttpContext context) =>
{
    var userId = context.User.FindFirst("sub")?.Value;
    var userName = context.User.Identity?.Name;
    return Results.Ok($"Welcome {userName} (ID: {userId}), this is protected data.");
});

// Allowed: Cristof (direct permission)
app.MapGet("/api/secured/users/delete", [Authorize(Policy = "FVP.Users.Users.Delete")] () =>
{
    return Results.Ok("Access granted for Users.Delete");
});

// Allowed: CristianVillalobos (direct permission)
app.MapGet("/api/secured/activates/update", [Authorize(Policy = "FVP.Associate.Activates.Update")] () =>
{
    return Results.Ok("Access granted for Activates.Update");
});

// Allowed: Cristof (direct permission)
app.MapGet("/api/secured/activates/view", [Authorize(Policy = "FVP.Associate.Activates.View")] () =>
{
    return Results.Ok("Access granted for Activates.View");
});

// Allowed: Editor role (Cristof, CristianVillalobos)
app.MapGet("/api/secured/pension-requirements/update", [Authorize(Policy = "FVP.Associate.PensionRequirements.Update")] () =>
{
    return Results.Ok("Access granted for PensionRequirements.Update");
});

AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.WriteLine("Shutting down...");

Console.WriteLine("Application has reached app.Run()");

app.Run();