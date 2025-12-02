using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record class UpdateConceptInput(
        [property: GraphQLName("ConceptoId")]
        long ConceptId,

        [property: GraphQLName("CuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("CuentaCredito")]
        string? CreditAccount);
}

