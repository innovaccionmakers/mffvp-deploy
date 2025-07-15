namespace Operations.Domain.SubtransactionTypes;

public interface ISubtransactionTypeRepository
{
    Task<SubtransactionType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);

    Task<SubtransactionType?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SubtransactionType>> GetCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyCollection<SubtransactionType>> GetAllAsync(
        CancellationToken cancellationToken = default);
}