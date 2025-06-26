using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("ContribucionOrigen")]
public record OriginContributionDto(
    [property: GraphQLName("id")] string Id,
    [property: GraphQLName("nombre")] string Name
);
