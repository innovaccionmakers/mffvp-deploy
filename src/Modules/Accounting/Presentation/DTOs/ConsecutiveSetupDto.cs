using HotChocolate;

namespace Accounting.Presentation.DTOs;

public sealed record ConsecutiveSetupDto(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("naturaleza")] string Nature,
    [property: GraphQLName("documentoFuente")] string SourceDocument,
    [property: GraphQLName("consecutivo")] int Consecutive);
