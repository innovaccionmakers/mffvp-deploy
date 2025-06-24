namespace Operations.Presentation.DTOs;

[GraphQLName("TipoTransaccion")]
public record TransactionTypeDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode,
    [property: GraphQLName("subtipos")] List<TransactionSubtypesDto> Subtypes
);

[GraphQLName("SubtipoTransaccion")]
public record TransactionSubtypesDto(
    [property: GraphQLName("id")] string Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);

