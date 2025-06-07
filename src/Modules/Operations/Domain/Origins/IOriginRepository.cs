namespace Operations.Domain.Origins;

public interface IOriginRepository
{
    Task<Origin?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);
}