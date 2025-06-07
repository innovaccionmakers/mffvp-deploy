namespace Operations.Domain.Channels;

public interface IChannelRepository
{
    Task<Channel?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);
}