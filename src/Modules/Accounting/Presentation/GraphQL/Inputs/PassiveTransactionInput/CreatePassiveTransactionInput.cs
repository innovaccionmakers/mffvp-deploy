using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public record class CreatePassiveTransactionInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("tipoOperacionesId")]
        long TypeOperationsId,

        [property: GraphQLName("cuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("cuentaCredito")]
        string? CreditAccount,

        [property: GraphQLName("cuentaContraCredito")]
        string? ContraCreditAccount,

        [property: GraphQLName("cuentaContraDebito")]
        string? ContraDebitAccount
        );
}
