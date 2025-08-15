using HotChocolate;

namespace Products.Presentation.DTOs;
public record class OfficeDto(
    [property: GraphQLName("id")] int Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("prefijo")] string Prefix,
    [property: GraphQLName("ciudad")] string City,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologado")] string HomologationCode,
    [property: GraphQLName("centroCostos")] string CostCenter
);
