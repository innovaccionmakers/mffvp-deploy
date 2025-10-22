namespace Operations.Domain.OperationTypes;

public interface IOperationTypeRepository
{
    Task<OperationType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);

    Task<OperationType?> GetByIdAsync(
        long operationTypeId,
        CancellationToken cancellationToken = default);

    Task<OperationType?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<OperationType?> GetByNameAndCategoryAsync(
        string name,
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OperationType>> GetTypesByCategoryAsync(
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OperationType>> GetAllAsync(
        CancellationToken cancellationToken = default);
}