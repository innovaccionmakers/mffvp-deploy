using HotChocolate;


namespace Closing.Presentation.GraphQL.Inputs
{
    public record RunSimulationInput(
        [property: GraphQLName("idPortafolio")] int PortfolioId,
        [property: GraphQLName("fechaCierre")] DateTime ClosingDate,
        [property: GraphQLName("estaCerrado")] bool IsClosing
        );
}
