using Operations.Integrations.OperationTypes;

namespace Operations.IntegrationEvents.OperationTypes;

public sealed record GetOperationTypeByNameResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    OperationTypeResponse? OperationType
);
