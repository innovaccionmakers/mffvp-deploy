using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs;

public sealed record ConsecutiveSetupInput(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("naturaleza")] string Nature,
    [property: GraphQLName("documentoFuente")] string SourceDocument,
    [property: GraphQLName("consecutivo")] int Consecutive);
