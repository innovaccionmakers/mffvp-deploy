var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () =>
{
    return new
    {
        service = "MakersBFF Gateway - True Modular Monolith",
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        architecture = "Schema Stitching Gateway",
        endpoints = new
        {
            graphql = "/graphql",
            playground = app.Environment.IsDevelopment() ? "/graphql" : null,
            health = "/health"
        },
        modules = new[]
        {
            "Operations Experience API",
            "Products Experience API",
            "Associate Experience API",
            "Customers Experience API"
        }
    };
});

app.Run();
