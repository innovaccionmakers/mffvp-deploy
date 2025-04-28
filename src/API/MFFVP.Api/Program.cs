using System.Reflection;
using Common.SharedKernel.Application;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Extensions;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();


Assembly[] moduleApplicationAssemblies = [
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

builder.Configuration.AddModuleConfiguration([]);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowSwaggerUI");
}

app.UseLogContext();

app.UseSerilogRequestLogging();

app.MapEndpoints();

app.Run();