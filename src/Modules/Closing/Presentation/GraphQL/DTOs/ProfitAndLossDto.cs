using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

public record ProfitAndLossDto(
    [property: GraphQLName("conceptos")] IReadOnlyDictionary<string, decimal> Values,
    [property: GraphQLName("rendimientosNetos")] decimal NetYield
);