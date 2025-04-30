using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.ClientOperations;
using Contributions.Integrations.ClientOperations.CreateClientOperation;
using Contributions.Integrations.ClientOperations.DeleteClientOperation;
using Contributions.Integrations.ClientOperations.GetClientOperation;
using Contributions.Integrations.ClientOperations.GetClientOperations;
using Contributions.Integrations.ClientOperations.UpdateClientOperation;

using MFFVP.Api.Application.Contributions;

namespace MFFVP.Api.Infrastructure.Contributions
{
    public sealed class ClientOperationsService : IClientOperationsService
    {
        public async Task<Result<IReadOnlyCollection<ClientOperationResponse>>> GetClientOperationsAsync(ISender sender)
        {
            return await sender.Send(new GetClientOperationsQuery());
        }

        public async Task<Result<ClientOperationResponse>> GetClientOperationAsync(Guid id, ISender sender)
        {
            return await sender.Send(new GetClientOperationQuery(id));
        }

        public async Task<Result> CreateClientOperationAsync(CreateClientOperationCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateClientOperationAsync(UpdateClientOperationCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteClientOperationAsync(Guid id, ISender sender)
        {
            return await sender.Send(new DeleteClientOperationCommand(id));
        }
    }
}