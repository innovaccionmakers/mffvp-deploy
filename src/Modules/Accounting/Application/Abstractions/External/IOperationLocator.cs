using Common.SharedKernel.Domain;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

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
}
