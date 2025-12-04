using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public record class CreatePassiveTransactionInput(
        [property: GraphQLName("PortafolioId")]
        int PortfolioId,

        [property: GraphQLName("TipoOperacionesId")]
        long TypeOperationsId,

        [property: GraphQLName("CuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("CuentaCredito")]
        string? CreditAccount,

        [property: GraphQLName("CuentaContraCredito")]
        string? ContraCreditAccount,

        [property: GraphQLName("CuentaContraDebito")]
        string? ContraDebitAccount
        );
}
