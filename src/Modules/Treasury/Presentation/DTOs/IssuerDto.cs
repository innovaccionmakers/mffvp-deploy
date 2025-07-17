using HotChocolate;

namespace Treasury.Presentation.DTOs;

public record IssuerDto(
    [property: GraphQLName("emisorId")] long Id,
    [property: GraphQLName("emisor")] string IssuerCode,
    [property: GraphQLName("descripcion")] string Description,
    [property: GraphQLName("nit")] float Nit,
    [property: GraphQLName("digito")] int Digit,
    [property: GraphQLName("codigoHomologado")] string HomologatedCode
);