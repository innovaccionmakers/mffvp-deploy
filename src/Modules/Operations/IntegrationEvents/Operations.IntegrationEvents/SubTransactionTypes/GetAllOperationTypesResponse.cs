using Operations.Integrations.SubTransactionTypes;

namespace Operations.IntegrationEvents.SubTransactionTypes;

public sealed record GetAllOperationTypesResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<SubtransactionTypeResponse> Types);