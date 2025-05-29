
using Common.SharedKernel.Domain;
using MediatR;

using People.Integrations.People.UpdatePerson;

using MFFVP.Api.Application.People;

namespace MFFVP.Api.Infrastructure.People
{
    public sealed class PeopleService : IPeopleService
    {
        public async Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender)
        {
            return await sender.Send(request);
        }
    }
}