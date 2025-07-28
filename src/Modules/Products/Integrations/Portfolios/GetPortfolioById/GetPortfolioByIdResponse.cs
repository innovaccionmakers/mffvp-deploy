using System.Text.Json.Serialization;

namespace Products.Integrations.Portfolios.GetPortfolioById;
public sealed record GetPortfolioByIdResponse(
    [property: JsonPropertyName("IdPortfolio")]
    int IdPortfolio,
    [property: JsonPropertyName("NombrePortafolio")]
    string Name,
    [property: JsonPropertyName("NombreCortoPortafolio")]
    string ShortName,
    [property: JsonPropertyName("AporteMinimoInicial")]
    decimal InitialMinimumAmount,
    [property: JsonPropertyName("AporteMinimoAdicional")]
    decimal AditionalMinimumAmount,
    [property: JsonPropertyName("FechaActualProceso")]
    DateTime CurrentDateProcess
);