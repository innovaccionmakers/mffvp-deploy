namespace Products.Domain.Alternatives;

public interface IAlternativeRepository
{
    Task<IReadOnlyCollection<Alternative>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Alternative>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Alternative?> GetAsync(int alternativeId, CancellationToken cancellationToken = default);
    Task<Alternative?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default
    );
}