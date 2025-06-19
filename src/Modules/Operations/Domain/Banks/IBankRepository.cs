namespace Operations.Domain.Banks;

public interface IBankRepository
{
    Task<Bank?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);
}