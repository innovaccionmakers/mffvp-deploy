using HotChocolate;
using Microsoft.Extensions.Hosting;

namespace Products.Presentation.DTOs;

public sealed record TechnicalSheetDto(
    [property: GraphQLName("Fecha")] DateTime Date,
    [property: GraphQLName("Aportes")] decimal Contributions,
    [property: GraphQLName("Retiros")] decimal Withdrawals,
    [property: GraphQLName("Pyg bruto")] decimal GrossPnl,
    [property: GraphQLName("Gastos")] decimal Expenses,
    [property: GraphQLName("Comisión día")] decimal DailyCommission,
    [property: GraphQLName("Costo día")] decimal DailyCost,
    [property: GraphQLName("Rend abonados")] decimal CreditedYields,
    [property: GraphQLName("Rend bruto por unidad")] decimal GrossUnitYield,
    [property: GraphQLName("Costos por unidad")] decimal UnitCost,
    [property: GraphQLName("Vr unidad")] decimal UnitValue,
    [property: GraphQLName("Unidades")] decimal Units,
    [property: GraphQLName("Valor portafolio")] decimal PortfolioValue,
    [property: GraphQLName("Participes")] int Participants
);
