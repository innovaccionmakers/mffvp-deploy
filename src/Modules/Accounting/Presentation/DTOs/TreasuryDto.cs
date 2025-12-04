using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class TreasuryDto(
        [property: GraphQLName("CuentaBancaria")]
        string? BankAccount,
        [property: GraphQLName("CuentaDebito")]
        string? DebitAccount,
        [property: GraphQLName("CuentaCredito")]
        string? CreditAccount
        );
}
