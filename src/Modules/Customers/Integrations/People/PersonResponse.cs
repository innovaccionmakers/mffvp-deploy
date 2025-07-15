using Common.SharedKernel.Domain;

namespace Customers.Integrations.People;

public sealed record PersonResponse(
    long PersonId,
    Guid DocumentType,
    string HomologatedCode,
    string Identification,
    string FirstName,
    string MiddleName,
    string LastName,
    string SecondLastName,
    DateTime BirthDate,
    string Mobile,
    string FullName,
    int GenderId,
    int CountryOfResidenceId,
    int DepartmentId,
    int MunicipalityId,
    string Email,
    int EconomicActivityId,
    Status Status,
    string Address,
    bool IsDeclarant,
    int InvestorTypeId,
    int RiskProfileId
);

public sealed record PersonInformationResponse(
    long PersonId,
    Guid DocumentType,
    string DocumentTypeHomologatedCode,
    string Identification,
    string FullName,
    Status Status
);