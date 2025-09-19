using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.IntegrationEvents.ClientOperations
{
    public record GetAccountingOperationsValidationResponse(
        bool IsValid, 
        string? Code, 
        string? Message,
        IReadOnlyCollection<GetAccountingOperationsResponse> ClientOperations);
}
