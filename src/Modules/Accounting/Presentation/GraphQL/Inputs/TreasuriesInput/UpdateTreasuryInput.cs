using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class UpdateTreasuryInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,
        [property: GraphQLName("cuentaBancaria")]
        string BankAccount,
        [property: GraphQLName("cuentaDebito")]
        string? DebitAccount,
        [property: GraphQLName("cuentaCredito")]
        string? CreditAccount
        );
}
