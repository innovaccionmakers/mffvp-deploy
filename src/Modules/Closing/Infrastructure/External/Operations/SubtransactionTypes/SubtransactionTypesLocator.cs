using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Operations.IntegrationEvents.SubTransactionTypes;

namespace Closing.Infrastructure.External.Operations.SubtransactionTypes
{
    internal sealed class SubtransactionTypesLocator(IRpcClient rpcClient) : ISubtransactionTypesLocator
    {
        public async Task<Result<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>> GetAllSubtransactionTypesAsync(
            CancellationToken cancellationToken)
        {
            var request = new GetAllOperationTypesRequest();

            var response = await rpcClient.CallAsync<
                GetAllOperationTypesRequest,
                GetAllOperationTypesResponse>(
                request,
                cancellationToken
            );

            IReadOnlyCollection<SubtransactionTypesRemoteResponse>? movements = response.Types?.Select(c => new SubtransactionTypesRemoteResponse(
                SubtransactionTypeId: c.SubtransactionTypeId,
                Name: c.Name,
                Category: c.Category,
                Nature: c.Nature,
                Status: c.Status
                )).ToList();
            return response.Succeeded
                ? Result.Success(movements!)
                : Result.Failure<IReadOnlyCollection<SubtransactionTypesRemoteResponse>>(
                  Error.Validation(response.Code!, response.Message!));
        }
    }
}
