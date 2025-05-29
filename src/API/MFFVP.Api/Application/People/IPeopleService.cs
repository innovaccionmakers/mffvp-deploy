using Common.SharedKernel.Domain;
using MediatR;

using People.Integrations.People;
using People.Integrations.People.UpdatePerson;

namespace MFFVP.Api.Application.People
{
    public interface IPeopleService
    {
        Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender);
    }
}