using HotChocolate;

namespace Treasury.Presentation.DTOs;

public record BankDto(
    [property: GraphQLName("bancoId")] string BankId,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
