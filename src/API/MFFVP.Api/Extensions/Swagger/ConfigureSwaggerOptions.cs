using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MFFVP.Api.Extensions.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"MFFVP API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Description = "BFF Web – FullContributions"
                });
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }
}