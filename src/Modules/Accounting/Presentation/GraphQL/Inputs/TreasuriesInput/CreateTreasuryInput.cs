using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class CreateTreasuryInput(
        [property: GraphQLName("PortafolioId")]
        int PortfolioId,
        [property: GraphQLName("CuentaBancaria")]
        string BankAccount,
        [property: GraphQLName("CuentaDebito")]
        string? DebitAccount,
        [property: GraphQLName("CuentaCredito")]
        string? CreditAccount
        );
}
