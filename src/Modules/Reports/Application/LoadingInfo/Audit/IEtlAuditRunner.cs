using Common.SharedKernel.Domain;
using Reports.Domain.LoadingInfo.Audit.Dto;


namespace Reports.Application.LoadingInfo.Audit;

public interface IEtlAuditRunner
{
    Task<Result<EtlAuditRunResult>> RunAsync(
        string executionName,
        string correlationId,
        ExecutionParametersBuilder builder,
        IReadOnlyList<EtlWorkItem> workItems,
        CancellationToken cancellationToken);
}
