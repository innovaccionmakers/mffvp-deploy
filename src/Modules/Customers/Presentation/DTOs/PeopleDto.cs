using HotChocolate;

namespace Customers.Presentation.DTOs;

public record PeopleDto(
    [property: GraphQLName("nombreCompleto")] string FullName,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("tipoDocumento")] string IdentificationType
);
