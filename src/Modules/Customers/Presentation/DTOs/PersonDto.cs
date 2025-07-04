using HotChocolate;

namespace Customers.Presentation.DTOs;

public record PersonDto(
    [property: GraphQLName("idPersona")] long PersonId,
    [property: GraphQLName("nombreCompleto")] string FullName,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("UuidTipoDocumento")] Guid DocumentTypeUuid
);
