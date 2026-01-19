namespace Operations.IntegrationEvents.ClientOperations;

public sealed record class GetCollectionBankIdsByClientOperationIdsRequest(
    IEnumerable<long> ClientOperationIds
);

