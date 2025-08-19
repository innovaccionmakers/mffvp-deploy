using Operations.Integrations.OperationTypes;

namespace Operations.IntegrationEvents.OperationTypes;

public sealed record GetAllOperationTypesResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<OperationTypeResponse> Types);