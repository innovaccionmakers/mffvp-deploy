using Common.SharedKernel.Infrastructure.Validation;
using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class DeleteTreasuryInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("cuentaBancaria")]
        [property: MaxCharLength(20)]
        string BankAccount
        );
}
