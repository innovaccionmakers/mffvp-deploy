namespace Operations.Presentation.DTOs;

[GraphQLName("ModoOrigen")]
public record OriginModeDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
