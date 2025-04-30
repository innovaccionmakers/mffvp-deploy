using System.Reflection;
using Common.SharedKernel.Application;
using Common.SharedKernel.Infrastructure;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Presentation.Endpoints;
using Contributions.Infrastructure;
using MFFVP.Api.BffWeb.Contributions;
using MFFVP.Api.Extensions;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

/*
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
*/

app.UseCors("AllowSwaggerUI");


app.UseLogContext();

app.UseSerilogRequestLogging();

app.MapEndpoints();

app.Run();