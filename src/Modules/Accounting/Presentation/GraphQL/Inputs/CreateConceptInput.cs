using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record class CreateConceptInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("nombre")]
        string Name,

        [property: GraphQLName("cuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("cuentaCredito")]
        string? CreditAccount
        );
}

