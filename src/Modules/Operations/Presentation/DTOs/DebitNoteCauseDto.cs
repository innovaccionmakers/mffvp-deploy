using HotChocolate;

namespace Operations.Presentation.DTOs;

[GraphQLName("CausalNotaDebito")]
public record DebitNoteCauseDto(
    [property: GraphQLName("uuid")] Guid Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
