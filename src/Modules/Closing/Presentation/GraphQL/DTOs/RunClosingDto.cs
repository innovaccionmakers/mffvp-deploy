using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

    public record RunClosingDto
    (
        [property: GraphQLName("Ingresos")] decimal Income,
        [property: GraphQLName("Egresos")] decimal Expenses,
        [property: GraphQLName("Comision")] decimal Commissions,
        [property: GraphQLName("Costos")] decimal Costs,
        [property: GraphQLName("RendimientosAbonar")] decimal YieldToCredit,
        [property: GraphQLName("ValorUnidad")] decimal? UnitValue,
        [property: GraphQLName("RentabilidadDiaria")] decimal? DailyProfitability,
        [property: GraphQLName("TieneAdvertencias")] bool? HasWarnings,
        [property: GraphQLName("Advertencias")] IEnumerable<WarningItemDto>? Warnings
    );