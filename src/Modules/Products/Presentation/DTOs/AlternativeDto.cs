using HotChocolate;

namespace Products.Presentation.DTOs;

public record AlternativeDto(
    [property: GraphQLName("alternativaId")] int Id,
    [property: GraphQLName("tipoAlternativaId")] int AlternativeTypeId,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("descripcion")] string Description,
    [property: GraphQLName("estado")] bool State,
    [property: GraphQLName("codigoHomologado")] string HomologationCode
);
