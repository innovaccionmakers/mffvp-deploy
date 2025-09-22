using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.Integrations.People.GetPeopleByIdentifications;

namespace Customers.Application.People.GetPeopleByIdentification
{
    internal class GetPeopleByIdentificationsQueryHandle(
    IPersonRepository personRepository) : IQueryHandler<GetPeopleByIdentificationsRequestQuery, IReadOnlyCollection<GetPeopleByIdentificationsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetPeopleByIdentificationsResponse>>> Handle(
            GetPeopleByIdentificationsRequestQuery query,
            CancellationToken cancellationToken)
        {
            var person = await personRepository.GetPeoplebyIdentificationsAsync(query.Identifications, cancellationToken);

            var response = person
            .Select(p => new GetPeopleByIdentificationsResponse(
                p!.Identification,
                p.DocumentType,
                p.FullName))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetPeopleByIdentificationsResponse>>(response);
        }
    }
}
