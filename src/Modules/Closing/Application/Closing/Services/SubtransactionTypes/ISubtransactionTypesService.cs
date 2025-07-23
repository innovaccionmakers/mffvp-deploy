using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.SubtransactionTypes
{
    public interface ISubtransactionTypesService
    {
        Task<Result<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>> GetAllAsync(CancellationToken cancellationToken);
    }
}
