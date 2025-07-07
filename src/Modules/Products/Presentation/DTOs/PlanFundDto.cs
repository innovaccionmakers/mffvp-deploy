using HotChocolate;

namespace Products.Presentation.DTOs;

public record class PlanFundDto(
    [property: GraphQLName("planId")] string IdPlan,
    [property: GraphQLName("planNombre")] string PlanName,
    [property: GraphQLName("codigoHomologadoPlan")] string HomologationCodePlan,
    [property: GraphQLName("fondoId")] string IdFund,
    [property: GraphQLName("fondoNombre")] string FundName,
    [property: GraphQLName("codigoHomologadoFondo")] string HomologationCodeFund
);