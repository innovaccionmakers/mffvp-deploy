using System.Threading;
using System.Threading.Tasks;

namespace Operations.Application.Abstractions.Services.OperationState;

public interface IOperationStateService
{
    Task<int> GetActiveStateAsync(CancellationToken cancellationToken = default);
}
