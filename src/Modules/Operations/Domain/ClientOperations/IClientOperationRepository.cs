namespace Operations.Domain.ClientOperations;

public interface IClientOperationRepository
{
    Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientOperation?> GetAsync(long clientoperationId, CancellationToken cancellationToken = default);
    void Insert(ClientOperation clientoperation);
    void Update(ClientOperation clientoperation);
    void Delete(ClientOperation clientoperation);

    Task<bool> ExistsContributionAsync(
        int affiliateId,
        int objectiveId,
        int portfolioId,
        CancellationToken ct);

    Task<IEnumerable<ClientOperation>> GetClientOperationsByProcessDateAsync(DateTime processDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientOperation>> GetAccountingOperationsAsync(IEnumerable<int> PortfolioId, DateTime processDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientOperation>> GetAccountingDebitNoteOperationsAsync(IEnumerable<int> portfolioIds, DateTime processDate, CancellationToken cancellationToken = default);

    Task<bool> HasActiveLinkedOperationAsync(
        long clientOperationId,
        long operationTypeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsInRangeAsync(
        IReadOnlyCollection<long> contributionOperationTypeIds,
        int affiliateId,
        int objectiveId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClientOperation>> GetContributionOperationsAsync(
        IReadOnlyCollection<long> contributionOperationTypeIds,
        int affiliateId,
        int objectiveId,
        CancellationToken cancellationToken = default);
}