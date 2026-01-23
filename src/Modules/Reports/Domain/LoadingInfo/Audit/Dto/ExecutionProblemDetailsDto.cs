using System.Text.Json.Serialization;

namespace Reports.Domain.LoadingInfo.Audit.Dto;

public sealed record ExecutionProblemDetailsDto(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("detail")] string? Detail,
    [property: JsonPropertyName("instance")] string? Instance,
    [property: JsonPropertyName("traceId")] string? TraceId,
    [property: JsonPropertyName("extensions")] IReadOnlyDictionary<string, string>? Extensions
);
