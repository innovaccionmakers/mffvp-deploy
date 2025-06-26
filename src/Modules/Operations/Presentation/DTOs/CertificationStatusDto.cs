using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("EstadoCertificacion")]
public record CertificationStatusDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);