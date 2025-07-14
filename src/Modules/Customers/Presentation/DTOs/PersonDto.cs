using HotChocolate;

namespace Customers.Presentation.DTOs;

public record PersonDto(
    [property: GraphQLName("idPersona")] long PersonId,
    [property: GraphQLName("nombreCompleto")] string FullName,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("uuidTipoDocumento")] Guid DocumentTypeUuid,
    [property: GraphQLName("tipoDocumento")] string? DocumentType = null
);
