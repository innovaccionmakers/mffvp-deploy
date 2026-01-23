namespace Reports.Integrations.LoadingInfo.Commands;

public sealed record EtlStepResponse(
    string EtlName,
    long ReadRows,
    long InsertedRows,
    bool IsSuccess,
    string? ErrorCode,
    string? ErrorMessage);
