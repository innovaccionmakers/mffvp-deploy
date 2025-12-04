using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class PassiveTransactionsDto(
        [property: GraphQLName("Id")]
        long PassiveTransactionId,
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
