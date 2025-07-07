using Common.SharedKernel.Domain;
using HotChocolate;

namespace Associate.Presentation.DTOs;

public record PensionRequirementDto(
    [property: GraphQLName("IdRequisitoPension")] int PensionRequirementId,
    [property: GraphQLName("FechaInicio")] DateTime StartDate,
    [property: GraphQLName("FechaVencimiento")] DateTime ExpirationDate,
    [property: GraphQLName("Estado")] Status Status
);
