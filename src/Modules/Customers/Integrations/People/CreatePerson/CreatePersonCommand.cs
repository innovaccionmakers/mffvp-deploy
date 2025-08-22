using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;

namespace Customers.Integrations.People.CreatePerson;
public sealed record CreatePersonCommand(
    Guid DocumentType,
    string HomologatedCode,
    string Identification,
    string FirstName,
    string MiddleName,
    string LastName,
    string SecondLastName,
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
) : ICommand<PersonResponse>;