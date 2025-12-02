using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record class CreateConceptInput(
        [property: GraphQLName("PortafolioId")]
        int PortfolioId,

        [property: GraphQLName("Nombre")]
        string Name,

        [property: GraphQLName("CuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("CuentaCredito")]
        string? CreditAccount
        );
}

