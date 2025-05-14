using System.Reflection;
using Associate.Infrastructure;
using Asp.Versioning;
using Common.SharedKernel.Application;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Validation;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Filters;
using FluentValidation;
using MFFVP.Api.BffWeb.Associate;
using MFFVP.Api.BffWeb.Trusts;
using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using Trusts.Application;
using Trusts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient(typeof(IValidator<>), typeof(TechnicalValidator<>));
builder.Services.AddSingleton(typeof(TechnicalValidationFilter<>));

builder.Services.AddSwaggerGen();

Assembly[] moduleApplicationAssemblies =
[
    AssemblyReference.Assembly,
    Associate.Application.AssemblyReference.Assembly
];

builder.Services.AddApplication(moduleApplicationAssemblies);

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
var mongoDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("MongoDB");
var databaseConnectionStringSQL = builder.Configuration.GetConnectionStringOrThrow("SqlServerDatabase");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    databaseConnectionString,
    mongoDbConnectionString,
    databaseConnectionStringSQL);

builder.Configuration.AddModuleConfiguration(["trusts", "associate"]);

builder.Services.AddTrustsModule(builder.Configuration);
builder.Services.AddActivatesModule(builder.Configuration);

builder.Services.AddBffTrustsServices();
builder.Services.AddBffActivatesServices();

builder.Services.AddEndpoints(typeof(TrustsEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(AssociateEndpoints).Assembly);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(moduleApplicationAssemblies));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwaggerUI", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
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

app.UseLogContext();

app.UseSerilogRequestLogging();

app.MapEndpoints();

app.MapGet("/",
    () => Results.Ok(new { module = "MFFVP", version = $"v.{Assembly.GetExecutingAssembly().GetName().Version}" }));

AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.WriteLine("Shutting down...");

Console.WriteLine("Application has reached app.Run()");

app.Run();