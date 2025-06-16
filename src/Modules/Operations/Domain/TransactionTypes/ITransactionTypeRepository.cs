namespace Operations.Domain.TransactionTypes;

public interface ITransactionTypeRepository
{
    Task<IReadOnlyCollection<TransactionType>> GetTransactionTypesByTypeAsync(
        string type,
        CancellationToken cancellationToken = default);
}
