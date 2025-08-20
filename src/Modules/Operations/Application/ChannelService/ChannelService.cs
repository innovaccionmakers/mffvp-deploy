using Operations.Application.Abstractions.Services.Channel;
using Operations.Domain.Channels;

namespace Operations.Application.ChannelService;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;

    public ChannelService(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<string> GetChannelCodeAsync(string name, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetByNameAsync(name, cancellationToken);
        
        return channel?.HomologatedCode ?? throw new Exception($"Channel '{name}' not found.");
    }
}