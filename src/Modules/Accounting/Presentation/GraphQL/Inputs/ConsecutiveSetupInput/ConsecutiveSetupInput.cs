using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.ConsecutiveSetupInput;

public sealed record ConsecutiveSetupInput(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("documentoFuente")] string SourceDocument,
    [property: GraphQLName("consecutivo")] int Consecutive);
