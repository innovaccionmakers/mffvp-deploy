namespace Operations.Domain.Channels;

public interface IChannelRepository
{
    Task<Channel?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un canal por su nombre.
    /// </summary>
    /// <param name="name">Nombre del canal.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El canal encontrado o null si no existe.</returns>
    Task<Channel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}