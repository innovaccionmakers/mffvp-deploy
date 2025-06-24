namespace Products.Presentation.DTOs;

[GraphQLName("TipoDocumento")]
public record DocumentTypeDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);