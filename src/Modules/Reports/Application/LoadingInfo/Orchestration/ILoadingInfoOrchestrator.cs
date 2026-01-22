using Common.SharedKernel.Domain;
using Reports.Domain.LoadingInfo.Constants;
using Reports.Integrations.LoadingInfo.commands;


namespace Reports.Application.LoadingInfo.Orchestration;

public interface ILoadingInfoOrchestrator
{
    Task<Result<LoadingInfoRunResponse>> RunAsync(
      string executionId,
      EtlSelection etlSelection,
      DateTime closingDateUtc,
      int portfolioId,
      CancellationToken cancellationToken);
}
