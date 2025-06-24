namespace Products.Presentation.DTOs;


[GraphQLName("Banco")]
public record BankDto(
    [property: GraphQLName("idBanco")] string BankId,
    [property: GraphQLName("nombre")] string Name
);
