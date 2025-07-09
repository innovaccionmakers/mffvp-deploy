using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public sealed record CreatePensionRequirementInput(
    [property: GraphQLName("idTipoIdentificacion")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("fechaInicioReqPen")] DateTime? StartDateReqPen,
    [property: GraphQLName("fechaFinReqPen")] DateTime? EndDateReqPen
);
