using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.People;
using People.Integrations.People.CreatePerson;
using People.Integrations.People;
using People.Application.Abstractions.Data;
using People.Domain.Countries;
using People.Domain.EconomicActivities;

namespace People.Application.People.CreatePerson;

internal sealed class CreatePersonCommandHandler(
    ICountryRepository countryRepository,
    IEconomicActivityRepository economicactivityRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePersonCommand, PersonResponse>
{
    public async Task<Result<PersonResponse>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var country = await countryRepository.GetAsync(request.CountryId, cancellationToken);
        var economicactivity = await economicactivityRepository.GetAsync(request.EconomicActivityId, cancellationToken);

        if (country is null)
            return Result.Failure<PersonResponse>(CountryErrors.NotFound(request.CountryId));
        if (economicactivity is null)
            return Result.Failure<PersonResponse>(EconomicActivityErrors.NotFound(request.EconomicActivityId));


        var result = Person.Create(
            request.DocumentType,
            request.StandardCode,
            request.Identification,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.SecondLastName,
            request.IssueDate,
            request.IssueCityId,
            request.BirthDate,
            request.BirthCityId,
            request.Mobile,
            request.FullName,
            request.MaritalStatusId,
            request.GenderId,
            request.Email,
            country,
            economicactivity
        );

        if (result.IsFailure) return Result.Failure<PersonResponse>(result.Error);

        var person = result.Value;

        personRepository.Insert(person);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PersonResponse(
            person.PersonId,
            person.DocumentType,
            person.StandardCode,
            person.Identification,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.SecondLastName,
            person.IssueDate,
            person.IssueCityId,
            person.BirthDate,
            person.BirthCityId,
            person.Mobile,
            person.FullName,
            person.MaritalStatusId,
            person.GenderId,
            person.CountryId,
            person.Email,
            person.EconomicActivityId
        );
    }
}