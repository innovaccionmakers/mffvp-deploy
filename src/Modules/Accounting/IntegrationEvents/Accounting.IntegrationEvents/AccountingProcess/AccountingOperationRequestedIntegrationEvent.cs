using Common.SharedKernel.Application.EventBus;

namespace Accounting.IntegrationEvents.AccountingProcess;

public sealed class AccountingOperationRequestedIntegrationEvent(
    string user,
    string? email,
    string processType,
    string processId,
    DateTime startDate,
    DateTime processDate,
    IEnumerable<int> portfolioIds)
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow)
{
    public string User { get; init; } = user;
    public string? Email { get; init; } = email;
    public string ProcessType { get; init; } = processType;
    public string ProcessId { get; init; } = processId;
    public DateTime StartDate { get; init; } = startDate;
    public DateTime ProcessDate { get; init; } = processDate;
    public IEnumerable<int> PortfolioIds { get; init; } = portfolioIds;
}

