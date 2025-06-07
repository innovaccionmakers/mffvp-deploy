namespace Operations.Domain.SubtransactionTypes;

public interface ISubtransactionTypeRepository
{
    Task<SubtransactionType?> GetByHomologatedCodeAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default);
}