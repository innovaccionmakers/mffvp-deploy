using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public sealed record UpdatePensionRequirementInput(
    [property: GraphQLName("idTipoIdentificacion")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("idRequisitoPension")] int? PensionRequirementId,
    [property: GraphQLName("estado")] bool? Status
);
