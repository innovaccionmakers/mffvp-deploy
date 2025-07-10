using HotChocolate;

namespace Products.Presentation.DTOs;

[GraphQLName("TipoObjetivo")]
public record GoalTypeDto(
    [property: GraphQLName("tipoObjetivoId")] int Id,
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);