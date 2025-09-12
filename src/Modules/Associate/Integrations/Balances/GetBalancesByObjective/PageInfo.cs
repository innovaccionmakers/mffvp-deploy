using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.GetBalancesByObjective;

public sealed record PageInfo(
    [property: JsonPropertyName("registrosTotalesGenerados")] int TotalRecords,
    [property: JsonPropertyName("numeroPaginasTotales")] int TotalPages,
    [property: JsonPropertyName("registrosEfectivosPorPagina")] int RecordsPerPage
);
