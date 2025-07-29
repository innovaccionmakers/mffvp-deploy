using HotChocolate;

namespace Products.Presentation.DTOs;

[GraphQLName("Portafolio")]
public record PortfolioDto(
    [property: GraphQLName("fondo")] string Found,
    [property: GraphQLName("fondoId")] int FoundId,
    [property: GraphQLName("plan")] string Plan,
    [property: GraphQLName("planId")] int PlanId,
    [property: GraphQLName("alternativa")] string Alternative,
    [property: GraphQLName("alternativaId")] int AlternativeId,
    [property: GraphQLName("portafolio")] string Portfolio,
    [property: GraphQLName("portafolioId")] int PortfolioId
);