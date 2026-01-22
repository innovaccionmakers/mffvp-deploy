using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Reports.Application.LoadingInfo.Models;


namespace Reports.Application.LoadingInfo.Services.Orchestrator;

public sealed record EtlStepDefinition(
  string EtlName,
    Func<CancellationToken, Task<(long ReadRows, long InsertedRows, Error? Error)>> ExecuteAsync);
