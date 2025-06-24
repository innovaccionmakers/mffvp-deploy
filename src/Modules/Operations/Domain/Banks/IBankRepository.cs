namespace Operations.Domain.Banks;

public interface IBankRepository
{
    Task<Bank?> FindByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Bank>> GetBanksAsync(
        CancellationToken cancellationToken = default);
}