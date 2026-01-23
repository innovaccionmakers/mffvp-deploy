namespace Reports.Domain.LoadingInfo.Audit.Dto;

public sealed record EtlAuditRunResult(
    string CorrelationId,
    long AuditExecutionId,
    bool IsSuccess,
    string? FailedEtlName,
    string? ErrorCode,
    string? ErrorMessage
);