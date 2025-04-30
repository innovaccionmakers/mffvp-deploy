using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.ClientOperations;
using Contributions.Integrations.ClientOperations.CreateClientOperation;
using Contributions.Integrations.ClientOperations.UpdateClientOperation;

namespace MFFVP.Api.Application.Contributions
{
    public interface IClientOperationsService
    {
        Task<Result<IReadOnlyCollection<ClientOperationResponse>>> GetClientOperationsAsync(ISender sender);
        Task<Result<ClientOperationResponse>> GetClientOperationAsync(Guid id, ISender sender);
        Task<Result> CreateClientOperationAsync(CreateClientOperationCommand request, ISender sender);
        Task<Result> UpdateClientOperationAsync(UpdateClientOperationCommand request, ISender sender);
        Task<Result> DeleteClientOperationAsync(Guid id, ISender sender);
    }
}