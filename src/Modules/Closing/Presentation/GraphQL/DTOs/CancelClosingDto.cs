using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

public record CancelClosingDto(
      [property: GraphQLName("IdPortafolio")] int PortfolioId,
      [property: GraphQLName("FechaCierre")] DateTime ClosingDate,
      [property: GraphQLName("EstaCancelado")] bool? IsCanceled
  );