using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("CasualAnulacion")]
public record CancellationClauseDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
