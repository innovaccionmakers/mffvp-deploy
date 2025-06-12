namespace Customers.IntegrationEvents.DocumentTypeValidation;

public sealed record GetDocumentTypeIdByCodeRequest(
    string TypeIdHomologationCode
);