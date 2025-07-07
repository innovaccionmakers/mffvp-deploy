using Common.SharedKernel.Domain;
using HotChocolate;

namespace Associate.Presentation.DTOs;

public record PensionRequirementDto(
    [property: GraphQLName("idRequisitoPension")] int PensionRequirementId,
    [property: GraphQLName("fechaInicio")] DateTime StartDate,
    [property: GraphQLName("fechaVencimiento")] DateTime ExpirationDate,
    [property: GraphQLName("estado")] Status Status
);
