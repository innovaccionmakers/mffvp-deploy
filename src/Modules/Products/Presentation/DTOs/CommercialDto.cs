using HotChocolate;

namespace Products.Presentation.DTOs;

public record class CommercialDto(
    [property: GraphQLName("id")] int Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("prefijo")] string Prefix,
    [property: GraphQLName("estado")] string Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
