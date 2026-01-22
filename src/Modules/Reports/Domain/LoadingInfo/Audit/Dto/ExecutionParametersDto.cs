using System.Text.Json.Serialization;

namespace Reports.Domain.LoadingInfo.Audit.Dto;

public sealed record ExecutionParametersDto(
    [property: JsonPropertyName("etlSelection")] string EtlSelection,  
    [property: JsonPropertyName("etlNames")] IReadOnlyList<string> EtlNames,
    [property: JsonPropertyName("requestedBy")] string? RequestedBy,
    [property: JsonPropertyName("requestedAtUtc")] DateTimeOffset RequestedAtUtc,
    [property: JsonPropertyName("correlationId")] string? CorrelationId,
    [property: JsonPropertyName("inputs")] IReadOnlyDictionary<string, string> Inputs,
    [property: JsonPropertyName("options")] IReadOnlyDictionary<string, string>? Options,
    [property: JsonPropertyName("etlRuns")] IReadOnlyList<EtlRunSummaryDto>? EtlRuns
);