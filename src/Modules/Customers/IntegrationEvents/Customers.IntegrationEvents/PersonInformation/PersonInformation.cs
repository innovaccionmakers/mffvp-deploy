using Common.SharedKernel.Core.Primitives;

namespace Customers.IntegrationEvents.PersonInformation;

public sealed record PersonInformation(
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
    int RiskProfileId);