using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class PassiveTransactionsDto(
        [property: GraphQLName("id")]
        long PassiveTransactionId,
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
