namespace Operations.Presentation.DTOs;

[GraphQLName("TipoTransaccion")]
public record TransactionTypeDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);
