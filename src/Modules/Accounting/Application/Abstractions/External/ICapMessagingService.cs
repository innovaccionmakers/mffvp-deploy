namespace Accounting.Application.Abstractions.External;

public interface ICapMessagingService
{
    Task SendAccountingProcessResultAsync(string operationType, bool success, string message, CancellationToken cancellationToken = default);
}
