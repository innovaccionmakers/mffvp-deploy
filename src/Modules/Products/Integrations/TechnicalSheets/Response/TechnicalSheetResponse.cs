using System.Text.Json.Serialization;

namespace Products.Integrations.TechnicalSheets.Response;

public sealed record TechnicalSheetResponse(
    [property: JsonPropertyName("IdTechnicalSheet")]
    int TechnicalSheetId,
    [property: JsonPropertyName("IdPortfolio")]
    int PortfolioId,
    [property: JsonPropertyName("Fecha")]
    DateTime Date,
    [property: JsonPropertyName("Contribuciones")]
    decimal Contributions,
    [property: JsonPropertyName("Retiros")]
    decimal Withdrawals,
    [property: JsonPropertyName("PnlBruto")]
    decimal GrossPnl,
    [property: JsonPropertyName("Gastos")]
    decimal Expenses,
    [property: JsonPropertyName("ComisionDiaria")]
    decimal DailyCommission,
    [property: JsonPropertyName("CostoDiario")]
    decimal DailyCost,
    [property: JsonPropertyName("RendimientosAcreditados")]
    decimal CreditedYields,
    [property: JsonPropertyName("RendimientoUnitarioBruto")]
    decimal GrossUnitYield,
    [property: JsonPropertyName("CostoUnitario")]
    decimal UnitCost,
    [property: JsonPropertyName("ValorUnitario")]
    decimal UnitValue,
    [property: JsonPropertyName("Unidades")]
    decimal Units,
    [property: JsonPropertyName("ValorPortafolio")]
    decimal PortfolioValue,
    [property: JsonPropertyName("Participantes")]
    int Participants
);