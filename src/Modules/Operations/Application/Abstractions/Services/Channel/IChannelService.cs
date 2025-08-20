using Operations.Domain.Channels;

namespace Operations.Application.Abstractions.Services.Channel;

public interface IChannelService
{
    Task<string> GetChannelCodeAsync(string name, CancellationToken cancellationToken = default);
}