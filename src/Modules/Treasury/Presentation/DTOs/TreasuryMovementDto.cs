using HotChocolate;

namespace Treasury.Presentation.DTOs;

public record TreasuryMovementDto
(
    [property: GraphQLName("idPortafolio")] long PortfolioId,
    [property: GraphQLName("fechaCierre")] DateOnly ClosingDate,
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("distribucion")] string Distribution,
    [property: GraphQLName("valor")] decimal Value
);
