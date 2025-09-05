using HotChocolate;

namespace Closing.Presentation.GraphQL.Inputs;
public record ConfirmClosingInput(
   [property: GraphQLName("idPortafolio")] int PortfolioId,
   [property: GraphQLName("fechaCierre")] DateTime ClosingDate
   );
