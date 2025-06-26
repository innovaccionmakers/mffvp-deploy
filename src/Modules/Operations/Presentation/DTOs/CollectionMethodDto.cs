namespace Operations.Presentation.DTOs;

[GraphQLName("MetodoRecaudo")]
public record CollectionMethodDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);