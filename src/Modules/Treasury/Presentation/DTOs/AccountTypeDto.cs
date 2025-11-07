using HotChocolate;

namespace Treasury.Presentation.DTOs;

[GraphQLName("TipoCuenta")]
public record AccountTypeDto(
    [property: GraphQLName("tipoCuentaId")] int Id,
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);

