
namespace Operations.Presentation.DTOs;

[GraphQLName("SubtipoTransaccion")]
public record SubTransactionTypeDto(
    [property: GraphQLName("id")] string Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);
