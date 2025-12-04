using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public record class DeletePassiveTransactionInput(
        [property: GraphQLName("PortafolioId")] 
        int PortfolioId,

        [property: GraphQLName("TipoOperacionesId")] 
        long TypeOperationsId
        );
}
