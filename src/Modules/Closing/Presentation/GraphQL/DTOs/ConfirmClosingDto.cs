using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

public record ConfirmClosingDto
  (
      [property: GraphQLName("IdPortafolio")] int PortfolioId,
      [property: GraphQLName("FechaCierre")] DateTime ClosingDate,
      [property: GraphQLName("TieneAdvertencias")] bool? HasWarnings,
      [property: GraphQLName("Advertencias")] IEnumerable<WarningItemDto>? Warnings
  );