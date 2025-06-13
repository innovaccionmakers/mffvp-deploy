using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Integrations.People.CreatePerson;
using Customers.Integrations.People;
using Customers.Application.Abstractions.Data;

namespace Customers.Application.People.CreatePerson

{
    internal sealed class CreatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreatePersonCommand, PersonResponse>
    {
        public async Task<Result<PersonResponse>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = Person.Create(
                request.IdentificationType,
                request.HomologatedCode,
                request.Identification,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.SecondLastName,
                request.Mobile,
                request.FullName,
                request.GenderId,
                request.CountryOfResidenceId,
                request.DepartmentId,
                request.MunicipalityId,
                request.Email,
                request.EconomicActivityId,
                request.Status,
                request.Address,
                request.IsDeclarant,
                request.InvestorTypeId,
                request.RiskProfileId
            );

            if (result.IsFailure)
            {
                return Result.Failure<PersonResponse>(result.Error);
            }

            var person = result.Value;
            
            personRepository.Insert(person);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PersonResponse(
                person.PersonId,
                person.DocumentType,
                person.HomologatedCode,
                person.Identification,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.SecondLastName,
                person.Mobile,
                person.FullName,
                person.GenderId,
                person.CountryOfResidenceId,
                person.DepartmentId,
                person.MunicipalityId,
                person.Email,
                person.EconomicActivityId,
                person.Status,
                person.Address,
                person.IsDeclarant,
                person.InvestorTypeId,
                person.RiskProfileId
            );
        }
    }
}