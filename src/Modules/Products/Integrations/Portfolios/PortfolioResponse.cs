using System.Text.Json.Serialization;

namespace Products.Integrations.Portfolios;

public sealed record PortfolioResponse(
    [property: JsonPropertyName("IdPortfolio")]
    long PortfolioId,
    [property: JsonPropertyName("CodigoHomologado")]
    string HomologatedCode,
    [property: JsonPropertyName("Nombre")]
    string Name,
    [property: JsonPropertyName("NombreCorto")]
    string ShortName,
    [property: JsonPropertyName("IdModalidad")]
    int ModalityId,
    [property: JsonPropertyName("MontoMinimoInicial")]
    decimal InitialMinimumAmount,
    [property: JsonPropertyName("FechaActual")]
    DateTime CurrentDate
);