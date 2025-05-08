using System.Reflection;
using Asp.Versioning;
using Common.SharedKernel.Application;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Presentation.Endpoints;
using Contributions.Infrastructure;
using MFFVP.Api.BffWeb.Contributions;
using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;
using FluentValidation;
using Serilog;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient(typeof(IValidator<>), typeof(TechnicalValidator<>));
builder.Services.AddSingleton(typeof(TechnicalValidationFilter<>));

builder.Services.AddSwaggerGen();

Assembly[] moduleApplicationAssemblies = [
    Contributions.Application.AssemblyReference.Assembly,
];

builder.Services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
string mongoDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("MongoDB");
string databaseConnectionStringSQL = builder.Configuration.GetConnectionStringOrThrow("SqlServerDatabase");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    databaseConnectionString,
    mongoDbConnectionString,
    databaseConnectionStringSQL);

builder.Configuration.AddModuleConfiguration(["contributions"]);

builder.Services.AddContributionsModule(builder.Configuration);

builder.Services.AddBffContributionsServices();

builder.Services.AddEndpoints(typeof(ContributionsEndpoints).Assembly);

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
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});




app.UseCors("AllowSwaggerUI");

app.UseLogContext();

app.UseSerilogRequestLogging();

//app.MapEndpoints();


app.MapGet("/", () => Results.Ok("Running on Railway"));
app.MapGet("/ping", () => Results.Ok("pong"));

AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.WriteLine("Shutting down...");

Console.WriteLine("✅ Application has reached app.Run()");

app.Run();