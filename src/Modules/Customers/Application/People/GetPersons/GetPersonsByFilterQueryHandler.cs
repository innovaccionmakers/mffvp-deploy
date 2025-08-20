using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Customers.Domain.People;
using Customers.Integrations.People;
using Customers.Integrations.People.GetPersons;

namespace Customers.Application.People.GetPersons;

internal sealed class GetPersonsByFilterQueryHandler(IPersonRepository repository) : IQueryHandler<GetPersonsByFilterQuery, IReadOnlyCollection<PersonInformationResponse>>
{
    public async Task<Result<IReadOnlyCollection<PersonInformationResponse>>> Handle(GetPersonsByFilterQuery request, CancellationToken cancellationToken)
    {
        var persons = await repository.GetActivePersonsByFilterAsync(request.IdentificationType, request.SearchBy, request.Text, cancellationToken);

        var response = persons
            .Select(e => new PersonInformationResponse(
                e.PersonId,
                e.DocumentType,
                e.DocumentTypeHomologatedCode,
                e.Identification,
                e.FullName,
                e.Status
               ))
            .ToList();

        return Result.Success<IReadOnlyCollection<PersonInformationResponse>>(response);
    }
}
