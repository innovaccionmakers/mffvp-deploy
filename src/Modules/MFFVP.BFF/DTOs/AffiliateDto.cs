using HotChocolate;

namespace MFFVP.BFF.DTOs;

public record AffiliateDto(
    [property: GraphQLName("nombreCompleto")] string FullName,
    [property: GraphQLName("identificacion")] string Identication,
    [property: GraphQLName("tipoDocumento")] string DocumentType,
    [property: GraphQLName("uuidTipoDocumento")] Guid DocumentTypeUuid,
    [property: GraphQLName("idAfiliado")] int AffiliateId,
    [property: GraphQLName("esPensionado")] bool Pensioner,
    [property: GraphQLName("fechaActivacion")] DateTime ActivateDate
);
