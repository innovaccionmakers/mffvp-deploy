namespace Operations.Presentation.DTOs;

[GraphQLName("MetodoRecaudo")]
public record CollectionMethodDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);