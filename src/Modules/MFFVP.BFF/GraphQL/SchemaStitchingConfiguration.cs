using MFFVP.BFF.Middlewares;

namespace MFFVP.BFF.GraphQL;

public static class SchemaStitchingConfiguration
{

    public static IServiceCollection AddSchemaStitching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddGraphQLServer("BFFGateway")
            .AddAuthorization()
            .UseField<Middlewares.GraphQLAuthorizePolicyItemsMiddleware>()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .BindRuntimeType <DateTime, Common.SharedKernel.Presentation.GraphQlBinding.DateTimeType>()//configura el manejo personalizado de DateTime
            .ModifyOptions(options =>
            {
                options.RemoveUnreachableTypes = true;
                options.StrictValidation = false;
                options.UseXmlDocumentation = true;
            })
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddGlobalObjectIdentification()
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
        if (configuration.GetValue("Development:EnablePlayground", true))
        {
            services.AddGraphQLServer("BFFGateway")
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
        services.AddGraphQLServer("BFFGateway")
            .ModifyRequestOptions(options =>
            {
                options.IncludeExceptionDetails = false;
                options.ExecutionTimeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
