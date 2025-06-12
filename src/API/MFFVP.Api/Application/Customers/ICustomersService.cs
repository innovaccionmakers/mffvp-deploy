using Common.SharedKernel.Domain;
using MediatR;

using Customers.Integrations.People;
using Customers.Integrations.People.UpdatePerson;
using Customers.Integrations.People.CreatePerson;

namespace MFFVP.Api.Application.Customers
{
    public interface ICustomersService
    {
        Task<Result<IReadOnlyCollection<PersonResponse>>> GetPersonsAsync(ISender sender);
        Task<Result> CreatePersonAsync(CreatePersonCommand request, ISender sender);
        Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender);
    }
}
