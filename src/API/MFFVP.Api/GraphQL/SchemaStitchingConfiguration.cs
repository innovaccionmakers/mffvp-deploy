using Common.SharedKernel.Presentation.GraphQL;
using Operations.Presentation.GraphQL;
using Products.Presentation.GraphQL;

namespace MFFVP.Api.GraphQL;

public static class SchemaStitchingConfiguration
{
    public static IServiceCollection AddSchemaStitching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGraphQLServer("BFFSuperExperience")
            .AddQueryType<RootQueryGraphQL>()
            .AddType<ProductsQueries>()
            .AddType<OperationsQueries>()
            .AddType<BffQueries>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyRequestOptions(options =>
            {
                options.IncludeExceptionDetails = true;
                options.ExecutionTimeout = TimeSpan.FromSeconds(30);
            });
        return services;
    }

    public static IServiceCollection AddDevelopmentConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuración adicional solo para desarrollo
        if (configuration.GetValue<bool>("Development:EnablePlayground", true))
        {
            services.AddGraphQLServer("BFFSuperExperience")
                .ModifyRequestOptions(options =>
                {
                    options.IncludeExceptionDetails = true;
                });
        }

        return services;
    }

    public static IServiceCollection AddProductionConfiguration(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddGraphQLServer("BFFSuperExperience")
            .ModifyRequestOptions(options =>
            {
                options.IncludeExceptionDetails = false;
                options.ExecutionTimeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
