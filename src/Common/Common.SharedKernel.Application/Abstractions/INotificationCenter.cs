namespace Common.SharedKernel.Application.Abstractions;

public interface INotificationCenter
{
    Task SendNotificationAsync(string message, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(string message, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);
    Task SendNotificationAsync<T>(T payload, CancellationToken cancellationToken = default) where T : class;
}
