using Common.SharedKernel.Infrastructure.Validation;
using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.TreasuriesInput
{
    public sealed record class CreateTreasuryInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("cuentaBancaria")]
        [property: MaxCharLength(20)]
        string BankAccount,

        [property: GraphQLName("cuentaDebito")]
        [property: MaxCharLength(20)]
        string? DebitAccount,

        [property: GraphQLName("cuentaCredito")]
        [property: MaxCharLength(20)]
        string? CreditAccount
    );
}
