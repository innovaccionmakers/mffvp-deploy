
using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("TipoOperacion")]
public record OperationTypeDto(
    [property: GraphQLName("id")] string Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
