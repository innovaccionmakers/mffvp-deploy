using Common.SharedKernel.Application.EventBus;

namespace Accounting.IntegrationEvents.AccountingProcess;

public sealed class AccountingProcessCompletedIntegrationEvent(
    string user,
    string processType,
    bool isSuccess,
    string? errorMessage,
    string processId,
    DateTime startDate,
    DateTime processDate,
    IEnumerable<int> portfolioIds)
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    public string User { get; set; } = user;
    public string ProcessType { get; init; } = processType;
    public bool IsSuccess { get; init; } = isSuccess;
    public string? ErrorMessage { get; init; } = errorMessage;
    public string ProcessId { get; init; } = processId;
    public DateTime StartDate { get; init; } = startDate;
    public DateTime ProcessDate { get; init; } = processDate;
    public IEnumerable<int> PortfolioIds { get; init; } = portfolioIds;
}
