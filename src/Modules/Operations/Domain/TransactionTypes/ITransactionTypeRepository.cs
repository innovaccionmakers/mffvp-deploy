namespace Operations.Domain.TransactionTypes;

public interface ITransactionTypeRepository
{
    Task<IReadOnlyCollection<TransactionType>> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default);
}
