using System.Text.Json.Serialization;

namespace Reports.Domain.LoadingInfo.Audit.Dto;

public sealed record EtlRunSummaryDto(
    [property: JsonPropertyName("etlName")] string EtlName,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("durationMs")] long? DurationMilliseconds,
    [property: JsonPropertyName("m")] IReadOnlyDictionary<string, long>? Metrics,
    [property: JsonPropertyName("w")] IReadOnlyList<string>? WarningCodes
);