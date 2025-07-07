using HotChocolate;

namespace Products.Presentation.DTOs.PlanFund;

public record class PlanDto(
    [property: GraphQLName("planId")] string IdPlan,
    [property: GraphQLName("planNombre")] string PlanName,
    [property: GraphQLName("codigoHomologadoPlan")] string HomologationCodePlan
);
