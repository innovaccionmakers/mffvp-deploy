using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class TreasuryDto(
        [property: GraphQLName("cuentaBancaria")]
        string? BankAccount,
        [property: GraphQLName("cuentaDebito")]
        string? DebitAccount,
        [property: GraphQLName("cuentaCredito")]
        string? CreditAccount
        );
}
