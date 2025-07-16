
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;

namespace Closing.Application.Abstractions.External.Operations.SubtransactionTypes
{
    public interface ISubtransactionTypesLocator
    {
        Task<Result<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>> GetAllSubtransactionTypesAsync(
            CancellationToken cancellationToken);
    }

    public sealed record SubtransactionTypesRemoteResponse(
        long SubtransactionTypeId,
        string Name,
        string Category,
        IncomeEgressNature Nature,
        Status Status
    );
}
