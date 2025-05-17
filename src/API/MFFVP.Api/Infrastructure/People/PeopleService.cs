
using Common.SharedKernel.Domain;
using MediatR;

using People.Integrations.People;
using People.Integrations.People.CreatePerson;
using People.Integrations.People.DeletePerson;
using People.Integrations.People.GetPerson;
using People.Integrations.People.GetPeople;
using People.Integrations.People.UpdatePerson;

using MFFVP.Api.Application.People;

namespace MFFVP.Api.Infrastructure.People
{
    public sealed class PeopleService : IPeopleService
    {
        public async Task<Result<IReadOnlyCollection<PersonResponse>>> GetPeopleAsync(ISender sender)
        {
            return await sender.Send(new GetPeopleQuery());
        }

        public async Task<Result<PersonResponse>> GetPersonAsync(long personId, ISender sender)
        {
            return await sender.Send(new GetPersonQuery(personId));
        }

        public async Task<Result> CreatePersonAsync(CreatePersonCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeletePersonAsync(long personId, ISender sender)
        {
            return await sender.Send(new DeletePersonCommand(personId));
        }
    }
}