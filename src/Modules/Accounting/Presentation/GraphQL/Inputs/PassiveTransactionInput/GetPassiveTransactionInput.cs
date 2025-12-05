using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public sealed record class GetPassiveTransactionInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("tipoOperacionesId")]
        long TypeOperationsId
        );
}
