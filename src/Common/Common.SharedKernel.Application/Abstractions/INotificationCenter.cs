namespace Common.SharedKernel.Application.Abstractions;

public interface INotificationCenter
{
    Task SendNotificationAsync(string user, string message, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(string user, string message, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);
    Task SendNotificationAsync<T>(string user, T payload, CancellationToken cancellationToken = default) where T : class;
}
