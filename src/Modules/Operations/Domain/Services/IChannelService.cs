using Operations.Domain.Channels;

namespace Operations.Domain.Services;

public interface IChannelService
{
    Task<string> GetChannelCodeAsync(string name, CancellationToken cancellationToken = default);
}