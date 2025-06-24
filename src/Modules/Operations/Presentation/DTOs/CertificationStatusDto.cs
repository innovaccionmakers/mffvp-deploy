namespace Operations.Presentation.DTOs;

[GraphQLName("EstadoCertificacion")]
public record CertificationStatusDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);