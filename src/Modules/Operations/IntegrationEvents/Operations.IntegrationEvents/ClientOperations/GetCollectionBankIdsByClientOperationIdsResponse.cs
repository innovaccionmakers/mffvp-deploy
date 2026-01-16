namespace Operations.IntegrationEvents.ClientOperations;

public sealed record class GetCollectionBankIdsByClientOperationIdsResponse(
    bool IsValid,
    string? Code,
    string? Message,
    Dictionary<long, int> CollectionBankIdsByClientOperationId
);

