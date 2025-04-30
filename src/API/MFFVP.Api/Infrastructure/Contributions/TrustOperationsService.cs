using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.TrustOperations;
using Contributions.Integrations.TrustOperations.CreateTrustOperation;
using Contributions.Integrations.TrustOperations.DeleteTrustOperation;
using Contributions.Integrations.TrustOperations.GetTrustOperation;
using Contributions.Integrations.TrustOperations.GetTrustOperations;
using Contributions.Integrations.TrustOperations.UpdateTrustOperation;

using MFFVP.Api.Application.Contributions;

namespace MFFVP.Api.Infrastructure.Contributions
{
    public sealed class TrustOperationsService : ITrustOperationsService
    {
        public async Task<Result<IReadOnlyCollection<TrustOperationResponse>>> GetTrustOperationsAsync(ISender sender)
        {
            return await sender.Send(new GetTrustOperationsQuery());
        }

        public async Task<Result<TrustOperationResponse>> GetTrustOperationAsync(Guid id, ISender sender)
        {
            return await sender.Send(new GetTrustOperationQuery(id));
        }

        public async Task<Result> CreateTrustOperationAsync(CreateTrustOperationCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateTrustOperationAsync(UpdateTrustOperationCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteTrustOperationAsync(Guid id, ISender sender)
        {
            return await sender.Send(new DeleteTrustOperationCommand(id));
        }
    }
}