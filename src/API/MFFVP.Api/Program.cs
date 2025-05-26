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
using MFFVP.Api.BffWeb.Operations;
using MFFVP.Api.BffWeb.People;
using MFFVP.Api.BffWeb.Products;
using MFFVP.Api.BffWeb.Trusts;
using MFFVP.Api.Extensions;
using MFFVP.Api.Extensions.Swagger;
using MFFVP.Api.MiddlewareExtensions;
using MFFVP.Api.OpenTelemetry;
using Operations.Infrastructure;
using People.Infrastructure;
using Products.Infrastructure;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
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
    Associate.Application.AssemblyReference.Assembly,
    Trusts.Application.AssemblyReference.Assembly,
    Products.Application.AssemblyReference.Assembly,
    People.Application.AssemblyReference.Assembly,
    Operations.Application.AssemblyReference.Assembly,
];

builder.Services.AddApplication(moduleApplicationAssemblies);

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
var mongoDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("MongoDB");
var databaseConnectionStringSQL = builder.Configuration.GetConnectionStringOrThrow("SqlServerDatabase");
var capDbConnectionString = builder.Configuration.GetConnectionStringOrThrow("CapDatabase");

builder.Services.AddInfrastructure(
    DiagnosticsConfig.ServiceName,
    databaseConnectionString,
    capDbConnectionString,
    databaseConnectionStringSQL);

builder.Configuration.AddModuleConfiguration(["trusts", "associate", "products", "people", "operations"]);

builder.Services.AddTrustsModule(builder.Configuration);
builder.Services.AddActivatesModule(builder.Configuration);
builder.Services.AddProductsModule(builder.Configuration);
builder.Services.AddPeopleModule(builder.Configuration);
builder.Services.AddOperationsModule(builder.Configuration);

builder.Services.AddBffTrustsServices();
builder.Services.AddBffActivatesServices();
builder.Services.AddBffProductsServices();
builder.Services.AddBffPeopleServices();
builder.Services.AddBffOperationsServices();

builder.Services.AddEndpoints(typeof(TrustsEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(AssociateEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(ProductsEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(PeopleEndpoints).Assembly);
builder.Services.AddEndpoints(typeof(OperationsEndpoints).Assembly);

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