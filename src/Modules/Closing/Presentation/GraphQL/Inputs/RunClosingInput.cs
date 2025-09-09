using HotChocolate;

namespace Closing.Presentation.GraphQL.Inputs;
public record RunClosingInput(
   [property: GraphQLName("idPortafolio")] int PortfolioId,
   [property: GraphQLName("fechaCierre")] DateTime ClosingDate
   );
