using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class GetTreasuryInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,
        [property: GraphQLName("cuentaBancaria")]
        string BankAccount
        );
}
