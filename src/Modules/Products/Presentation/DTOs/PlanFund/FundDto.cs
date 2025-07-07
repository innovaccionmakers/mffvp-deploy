using HotChocolate;

namespace Products.Presentation.DTOs.PlanFund;

public record class FundDto(
    [property: GraphQLName("fondoId")] string IdFund,
    [property: GraphQLName("fondoNombre")] string FundName,
    [property: GraphQLName("codigoHomologadoFondo")] string HomologationCodeFund
);
