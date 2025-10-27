using Common.SharedKernel.Domain;
using Operations.IntegrationEvents.ClientOperations;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Accounting.Application.Abstractions.External;

public interface IOperationLocator
{
    Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> GetAccountingOperationsAsync(IEnumerable<int> portfolioIds,
                                                                                                    DateTime processDate,
                                                                                                    CancellationToken cancellationToken);

    Task<Result<(long OperationTypeId, string Nature, string Name)>> GetOperationTypeByNameAsync(string name, CancellationToken cancellationToken);

    string GetEnumMemberValue(Enum value);
}
