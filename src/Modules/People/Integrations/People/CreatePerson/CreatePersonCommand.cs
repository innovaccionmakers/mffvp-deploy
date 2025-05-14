using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.People.CreatePerson;
public sealed record CreatePersonCommand(
    string DocumentType,
    string StandardCode,
    string Identification,
    string FirstName,
    string MiddleName,
    string LastName,
    string SecondLastName,
    DateTime IssueDate,
    int IssueCityId,
    DateTime BirthDate,
    int BirthCityId,
    string Mobile,
    string FullName,
    int MaritalStatusId,
    int GenderId,
    int CountryId,
    string Email,
    string EconomicActivityId
) : ICommand<PersonResponse>;