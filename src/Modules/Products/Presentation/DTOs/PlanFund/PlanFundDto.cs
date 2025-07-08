using HotChocolate;

namespace Products.Presentation.DTOs.PlanFund;

public record class PlanFundDto(
    [property: GraphQLName("plan")] PlanDto Plan,
    [property: GraphQLName("fondo")] FundDto Fondo
);