using HotChocolate;

namespace Products.Presentation.DTOs;

[GraphQLName("Portafolio")]
public record PortfolioDto(
    [property: GraphQLName("fondo")] string Found,
    [property: GraphQLName("plan")] string Plan,
    [property: GraphQLName("alternativa")] string Alternative,
    [property: GraphQLName("portafolio")] string Portfolio
);