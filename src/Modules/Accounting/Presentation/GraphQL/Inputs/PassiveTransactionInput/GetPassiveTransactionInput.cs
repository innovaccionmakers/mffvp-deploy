using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public sealed record class GetPassiveTransactionInput(
        [property: GraphQLName("PortafolioId")]
        int PortfolioId,

        [property: GraphQLName("TipoOperacionesId")]
        long TypeOperationsId
        );
}
