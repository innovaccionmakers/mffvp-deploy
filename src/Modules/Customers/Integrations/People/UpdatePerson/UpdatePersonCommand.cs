using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using System;

namespace Customers.Integrations.People.UpdatePerson;

public sealed record UpdatePersonCommand(
    long PersonId,
    Guid NewIdentificationType,
    string NewHomologatedCode,
    string NewIdentification,
    string NewFirstName,
    string NewMiddleName,
    string NewLastName,
    string NewSecondLastName,
    string NewMobile,
    string NewFullName,
    int NewGenderId,
    int NewCountryOfResidenceId,
    int NewDepartmentId,
    int NewMunicipalityId,
    string NewEmail,
    int NewEconomicActivityId,
    Status NewStatus,
    string NewAddress,
    bool NewIsDeclarant,
    int NewInvestorTypeId,
    int NewRiskProfileId
) : ICommand<PersonResponse>;