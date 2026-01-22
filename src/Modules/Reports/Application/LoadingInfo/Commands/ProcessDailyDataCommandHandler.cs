using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Domain.LoadingInfo.Constants;
using Reports.Integrations.LoadingInfo.Commands;
using Common.SharedKernel.Application.Helpers.Time;
using Reports.Application.LoadingInfo.Orchestration;

namespace Reports.Application.LoadingInfo.Commands;

public sealed class ProcessDailyDataCommandHandler
    : ICommandHandler<ProcessDailyDataCommand, ProcessDailyDataResponse>
{
    private readonly ILoadingInfoOrchestrator loadingInfoOrchestrator;
    private readonly ILogger<ProcessDailyDataCommandHandler> logger;

    public ProcessDailyDataCommandHandler(
        ILoadingInfoOrchestrator loadingInfoOrchestrator,
        ILogger<ProcessDailyDataCommandHandler> logger)
    {
        this.loadingInfoOrchestrator = loadingInfoOrchestrator;
        this.logger = logger;
    }

    public async Task<Result<ProcessDailyDataResponse>> Handle(
      ProcessDailyDataCommand request,
   CancellationToken cancellationToken)
    {
        var executionId = Guid.NewGuid().ToString("N");

        if (request.portfolioId <= 0)
        {
            return Result.Failure<ProcessDailyDataResponse>(
         new Error("REQ_001", "portfolioId debe ser un entero positivo.", ErrorType.Validation));
        }

        if (request.etlSelection == EtlSelection.None)
        {
            return Result.Failure<ProcessDailyDataResponse>(
             new Error("REQ_002", "etlSelection debe incluir al menos un ETL.", ErrorType.Validation));
        }

        var closingDateUtc = DateTimeConverter.ToUtcDateTime(request.closingDateUtc);

        logger.LogInformation(
           "ProcessDailyData inició. ExecutionId={ExecutionId} PortfolioId={PortfolioId} ClosingDateUtc={ClosingDateUtc} Selection={Selection}",
          executionId, request.portfolioId, closingDateUtc, request.etlSelection);

        var orchestratorResult = await loadingInfoOrchestrator.RunAsync(
            executionId,
            request.etlSelection,
            closingDateUtc,
            request.portfolioId,
            cancellationToken);

        if (orchestratorResult.IsFailure)
        {
            logger.LogWarning(
               "ProcessDailyData falló. ExecutionId={ExecutionId} ErrorCode={ErrorCode} ErrorMessage={ErrorMessage}",
                executionId,
                  orchestratorResult.Error.Code,
                orchestratorResult.Error.Description);

            return Result.Failure<ProcessDailyDataResponse>(orchestratorResult.Error);
        }

        var loadingInfoResponse = orchestratorResult.Value;

        var hasErrors = loadingInfoResponse.Steps.Any(s => !s.IsSuccess);

        var status = hasErrors
        ? "CompletedWithErrors"
         : "Success";

        var response = new ProcessDailyDataResponse
        {
            ExecutionId = loadingInfoResponse.CorrelationId,
            PortfolioId = request.portfolioId,
            ClosingDate = closingDateUtc,
            Selection = request.etlSelection,
            Status = status,
            Steps = loadingInfoResponse.Steps
        };

        logger.LogInformation(
              "ProcessDailyData finalizó. ExecutionId={ExecutionId} AuditExecutionId={AuditExecutionId} Status={Status} Steps={StepCount}",
                loadingInfoResponse.CorrelationId,
                loadingInfoResponse.AuditExecutionId,
                status,
                loadingInfoResponse.Steps.Count);

        return Result.Success(response);
    }
}