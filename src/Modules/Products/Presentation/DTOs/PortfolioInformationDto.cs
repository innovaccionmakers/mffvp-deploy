using HotChocolate;

namespace Products.Presentation.DTOs;

[GraphQLName("PortafolioInfo")]
public record PortfolioInformationDto(
    [property: GraphQLName("idPortfolio")] long Id,
    [property: GraphQLName("codigoHomologado")] string HomologationCode,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("nombreCorto")] string ShorName,
    [property: GraphQLName("idModalidad")] int ModalityId,
    [property: GraphQLName("montoMinimoInicial")] decimal MinimalAmountInit,
    [property: GraphQLName("fechaActualOperacion")] DateTime CurrentDateOperation
);