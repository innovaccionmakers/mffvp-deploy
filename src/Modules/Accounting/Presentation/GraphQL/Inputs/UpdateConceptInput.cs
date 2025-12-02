using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record class UpdateConceptInput(
        [property: GraphQLName("conceptoId")]
        long ConceptId,

        [property: GraphQLName("cuentaDebito")]
        string? DebitAccount,

        [property: GraphQLName("cuentaCredito")]
        string? CreditAccount);
}

