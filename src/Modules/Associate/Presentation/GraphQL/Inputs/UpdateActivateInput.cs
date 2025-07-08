using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public record UpdateActivateInput(
    [property: GraphQLName("tipoId")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("pensionado")] bool? Pensioner
);