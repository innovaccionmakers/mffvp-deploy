namespace People.IntegrationEvents.DocumentTypeValidation;

public sealed record GetDocumentTypeIdByCodeRequest(
    string TypeIdHomologationCode
    );