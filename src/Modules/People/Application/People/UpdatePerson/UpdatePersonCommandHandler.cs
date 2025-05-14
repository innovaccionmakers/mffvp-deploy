using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.People;
using People.Integrations.People.UpdatePerson;
using People.Integrations.People;
using People.Application.Abstractions.Data;

namespace People.Application.People;
internal sealed class UpdatePersonCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePersonCommand, PersonResponse>
{
    public async Task<Result<PersonResponse>> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await personRepository.GetAsync(request.PersonId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<PersonResponse>(PersonErrors.NotFound(request.PersonId));
        }

        entity.UpdateDetails(
            request.NewDocumentType, 
            request.NewStandardCode, 
            request.NewIdentification, 
            request.NewFirstName, 
            request.NewMiddleName, 
            request.NewLastName, 
            request.NewSecondLastName, 
            request.NewIssueDate, 
            request.NewIssueCityId, 
            request.NewBirthDate, 
            request.NewBirthCityId, 
            request.NewMobile, 
            request.NewFullName, 
            request.NewMaritalStatusId, 
            request.NewGenderId, 
            request.NewCountryId, 
            request.NewEmail, 
            request.NewEconomicActivityId
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PersonResponse(entity.PersonId, entity.DocumentType, entity.StandardCode, entity.Identification, entity.FirstName, entity.MiddleName, entity.LastName, entity.SecondLastName, entity.IssueDate, entity.IssueCityId, entity.BirthDate, entity.BirthCityId, entity.Mobile, entity.FullName, entity.MaritalStatusId, entity.GenderId, entity.CountryId, entity.Email, entity.EconomicActivityId);
    }
}