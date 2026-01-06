using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public record class DeletePassiveTransactionInput(
        [property: GraphQLName("portafolioId")] 
        int PortfolioId,

        [property: GraphQLName("tipoOperacionId")] 
        long TypeOperationId
        );
}
