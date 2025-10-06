namespace Accounting.Application.Abstractions.External;

public interface ICabMessagingService
{
    Task SendAccountingProcessResultAsync(string operationType, bool success, string message, CancellationToken cancellationToken = default);
}
