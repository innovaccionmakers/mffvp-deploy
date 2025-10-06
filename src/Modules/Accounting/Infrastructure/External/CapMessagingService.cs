using Accounting.Application.Abstractions.External;

namespace Accounting.Infrastructure.External;

public class CapMessagingService : ICapMessagingService
{
    public async Task SendAccountingProcessResultAsync(string operationType, bool success, string message, CancellationToken cancellationToken = default)
    {      
        await Task.CompletedTask;
    }
}
