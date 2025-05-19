using Common.SharedKernel.Domain;
using MediatR;

using People.Integrations.People;
using People.Integrations.People.CreatePerson;
using People.Integrations.People.UpdatePerson;

namespace MFFVP.Api.Application.People
{
    public interface IPeopleService
    {
        Task<Result<IReadOnlyCollection<PersonResponse>>> GetPeopleAsync(ISender sender);
        Task<Result<PersonResponse>> GetPersonAsync(long personId, ISender sender);
        Task<Result> CreatePersonAsync(CreatePersonCommand request, ISender sender);
        Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender);
        Task<Result> DeletePersonAsync(long personId, ISender sender);
    }
}