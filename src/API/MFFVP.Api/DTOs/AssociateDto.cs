using HotChocolate;

namespace MFFVP.Api.DTOs;

[GraphQLName("Afiliado")]
public sealed record AssociateDto(
    [property: GraphQLName("tipoIdentificacion")] string IdentificationType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("nombreCompleto")] string FullName
);
