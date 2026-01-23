

using Reports.Integrations.LoadingInfo.Commands;

namespace Reports.Integrations.LoadingInfo.commands;
public sealed record LoadingInfoRunResponse(
    string CorrelationId,
    long AuditExecutionId,
    bool IsSuccess,
    IReadOnlyList<EtlStepResponse> Steps,
    string? ErrorCode,
    string? ErrorMessage
);