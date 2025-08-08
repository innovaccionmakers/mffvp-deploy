namespace MFFVP.BFF.DTOs;

public sealed record TreasuryMovementByPortfoliosDto(
    [property: GraphQLName("portafolio")] string Portfolio,
    [property: GraphQLName("fechaCierre")] DateOnly ClosingDate,
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("distribucion")] string Distribution,
    [property: GraphQLName("valor")] decimal Value
);
