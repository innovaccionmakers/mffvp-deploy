using Common.SharedKernel.Infrastructure.Validation;
using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput
{
    public record class UpdatePassiveTransactionInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("tipoOperacionesId")]
        long TypeOperationsId,

        [property: GraphQLName("cuentaDebito")]
        [property: MaxCharLength(20)]
        string? DebitAccount,

        [property: GraphQLName("cuentaCredito")]
        [property: MaxCharLength(20)]
        string? CreditAccount,

        [property: GraphQLName("cuentaContraCredito")]
        [property: MaxCharLength(20)]
        string? ContraCreditAccount,

        [property: GraphQLName("cuentaContraDebito")]
        [property: MaxCharLength(20)]
        string? ContraDebitAccount);
}
