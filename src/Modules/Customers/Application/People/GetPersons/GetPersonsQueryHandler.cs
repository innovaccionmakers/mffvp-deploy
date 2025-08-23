using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.Integrations.People.GetPersons;

namespace Customers.Application.People.GetPersons;

internal sealed class GetPersonsQueryHandler(
    IPersonRepository personRepository)
    : IQueryHandler<GetPersonsQuery, IReadOnlyCollection<PersonResponse>>
{
    public async Task<Result<IReadOnlyCollection<PersonResponse>>> Handle(GetPersonsQuery request, CancellationToken cancellationToken)
    {
        var entities = await personRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new PersonResponse(
                e.PersonId,
                e.DocumentType,
                e.HomologatedCode,
                e.Identification,
                e.FirstName,
                e.MiddleName,
                e.LastName,
                e.SecondLastName,
                e.BirthDate,
                e.Mobile,
                e.FullName,
                e.GenderId,
                e.CountryOfResidenceId,
                e.DepartmentId,
                e.MunicipalityId,
                e.Email,
                e.EconomicActivityId,
                e.Status,
                e.Address,
                e.IsDeclarant,
                e.InvestorTypeId,
                e.RiskProfileId))
            .ToList();

        return Result.Success<IReadOnlyCollection<PersonResponse>>(response);
    }
}