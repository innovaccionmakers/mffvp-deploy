using Common.SharedKernel.Application.EventBus;

namespace Accounting.IntegrationEvents.AccountingInconsistencies;

public sealed class AccountingServiceResultIntegrationEvent : IntegrationEvent
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public string ProcessType { get; init; }

    public AccountingServiceResultIntegrationEvent(
        bool success, string message, string processType) : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        Success = success;
        Message = message;
        ProcessType = processType;
    }
}
