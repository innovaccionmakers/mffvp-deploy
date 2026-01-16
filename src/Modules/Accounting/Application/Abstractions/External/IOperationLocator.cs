using Common.SharedKernel.Domain;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using Operations.Integrations.OperationTypes;

namespace Accounting.Application.Abstractions.External;

public interface IOperationLocator
{
    Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> GetAccountingOperationsAsync(IEnumerable<int> portfolioIds,
                                                                                                    DateTime processDate,
                                                                                                    CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> GetAccountingDebitNoteOperationsAsync(IEnumerable<int> portfolioIds,
                                                                                                    DateTime processDate,
                                                                                                    CancellationToken cancellationToken);


    Task<Result<(long OperationTypeId, string Nature, string Name)>> GetOperationTypeByNameAsync(string name, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<OperationTypeResponse>>>  GetOperationTypesByNameAsync(string name, CancellationToken cancellationToken);
    Task<Result<Dictionary<long, int>>> GetCollectionBankIdsByClientOperationIdsAsync(IEnumerable<long> clientOperationIds, CancellationToken cancellationToken);
}
