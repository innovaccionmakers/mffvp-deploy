using Microsoft.Extensions.DependencyInjection;

namespace Operations.Presentation.GraphQL;

public static class OperationsGraphQLConfiguration
{
    public static IServiceCollection AddOperationsGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<OperationsQueries>()
            .AddFiltering()
            .AddSorting()
            .AddProjections();

        return services;
    }
}