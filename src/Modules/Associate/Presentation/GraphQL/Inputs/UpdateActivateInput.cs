using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public record UpdateActivateInput(
    [property: GraphQLName("idTipoIdentificacion")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("pensionado")] bool? Pensioner
);