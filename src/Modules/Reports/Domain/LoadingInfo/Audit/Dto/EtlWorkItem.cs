using Common.SharedKernel.Domain;

namespace Reports.Domain.LoadingInfo.Audit.Dto;

public sealed record EtlWorkItem(
    string EtlName,
    Func<CancellationToken, Task<Result<EtlWorkResult>>> RunAsync
);

public sealed record EtlWorkResult(
    IReadOnlyDictionary<string, long>? Metrics = null,
    IReadOnlyList<string>? WarningCodes = null
);