namespace Customers.Integrations.People;

public sealed record PersonResponse(
    long PersonId,
    Guid IdentificationType,
    string HomologatedCode,
    string Identification,
    string FirstName,
    string MiddleName,
    string LastName,
    string SecondLastName,
    string Mobile,
    string FullName,
    Guid GenderId,
    int CountryOfResidenceId,
    int DepartmentId,
    int MunicipalityId,
    string Email,
    int EconomicActivityId,
    bool Status,
    string Address,
    bool IsDeclarant,
    Guid InvestorTypeId,
    Guid RiskProfileId
);