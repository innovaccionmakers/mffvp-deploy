using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class DeleteTreasuryInput(
        [property: GraphQLName("PortafolioId")]
        int PortfolioId,
        [property: GraphQLName("CuentaBancaria")]
        string BankAccount
        );
}
