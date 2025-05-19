using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.People.UpdatePerson;

public sealed record UpdatePersonCommand(
    long PersonId,
    string NewDocumentType,
    string NewStandardCode,
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