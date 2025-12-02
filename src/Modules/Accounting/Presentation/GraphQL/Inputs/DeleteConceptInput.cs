using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record class DeleteConceptInput(
        [property: GraphQLName("ConceptoId")]
        long ConceptId
    );
}

