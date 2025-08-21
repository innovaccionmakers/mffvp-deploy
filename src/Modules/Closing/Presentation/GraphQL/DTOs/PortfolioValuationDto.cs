using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

public sealed record PortfolioValuationDto(
    [property: GraphQLName("portafolioId")] int PortfolioId, 
    [property: GraphQLName("fechaCierre")] DateTime ClosingDate,
    [property: GraphQLName("aportes")] decimal Contributions,
    [property: GraphQLName("retiros")] decimal Withdrawals, 
    [property: GraphQLName("pygBruto")] decimal PygBruto, 
    [property: GraphQLName("gastos")] decimal Expenses,
    [property: GraphQLName("comisionDia")] decimal CommissionDay, 
    [property: GraphQLName("costoDia")] decimal CostDay, 
    [property: GraphQLName("rendimientosAbonados")] decimal CreditedYields,
    [property: GraphQLName("rendimientoBrutoUnidad")] decimal GrossYieldPerUnit,
    [property: GraphQLName("costosUnidad")] decimal CostPerUnit,
    [property: GraphQLName("valorUnidad")] decimal UnitValue,
    [property: GraphQLName("unidades")] decimal Units,
    [property: GraphQLName("valorPortafolio")] decimal AmountPortfolio,
    [property: GraphQLName("participes")] IEnumerable<long> TrustIds
);
