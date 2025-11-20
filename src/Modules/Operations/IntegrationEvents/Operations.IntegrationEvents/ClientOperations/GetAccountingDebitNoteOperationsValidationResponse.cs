using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.IntegrationEvents.ClientOperations;

public record GetAccountingDebitNoteOperationsValidationResponse(
    bool IsValid, 
    string? Code, 
    string? Message,
    IReadOnlyCollection<GetAccountingOperationsResponse> ClientOperations);
