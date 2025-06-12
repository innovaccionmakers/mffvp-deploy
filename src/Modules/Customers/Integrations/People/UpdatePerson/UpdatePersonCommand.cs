using Common.SharedKernel.Application.Messaging;
using System;

namespace Customers.Integrations.People.UpdatePerson;

public sealed record UpdatePersonCommand(
    long PersonId,
    string NewDocumentType,
    string NewHomologatedCode,
    string NewIdentification,
    string NewFirstName,
    string NewMiddleName,
    string NewLastName,
    string NewSecondLastName,
    DateTime NewIssueDate,
    int NewIssueCityId,
    DateTime NewBirthDate,
    int NewBirthCityId,
    string NewMobile,
    string NewFullName,
    int NewMaritalStatusId,
    int NewGenderId,
    int NewCountryId,
    string NewEmail,
    string NewEconomicActivityId
) : ICommand<PersonResponse>;