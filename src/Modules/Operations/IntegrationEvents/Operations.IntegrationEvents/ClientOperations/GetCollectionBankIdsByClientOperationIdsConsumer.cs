using Common.SharedKernel.Application.Rpc;
using Operations.Domain.AuxiliaryInformations;

namespace Operations.IntegrationEvents.ClientOperations;

public sealed class GetCollectionBankIdsByClientOperationIdsConsumer(
    IAuxiliaryInformationRepository auxiliaryInformationRepository)
    : IRpcHandler<GetCollectionBankIdsByClientOperationIdsRequest, GetCollectionBankIdsByClientOperationIdsResponse>
{
    public async Task<GetCollectionBankIdsByClientOperationIdsResponse> HandleAsync(
        GetCollectionBankIdsByClientOperationIdsRequest message,
        CancellationToken cancellationToken)
    {
        try
        {
            var collectionBankIds = await auxiliaryInformationRepository
                .GetCollectionBankIdsByClientOperationIdsAsync(message.ClientOperationIds, cancellationToken);

            return new GetCollectionBankIdsByClientOperationIdsResponse(
                IsValid: true,
                Code: null,
                Message: null,
                CollectionBankIdsByClientOperationId: collectionBankIds);
        }
        catch (Exception ex)
        {
            return new GetCollectionBankIdsByClientOperationIdsResponse(
                IsValid: false,
                Code: "Error",
                Message: ex.Message,
                CollectionBankIdsByClientOperationId: new Dictionary<long, int>());
        }
    }
}

