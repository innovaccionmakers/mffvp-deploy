using Common.SharedKernel.Domain;
using MediatR;

using Customers.Integrations.People;
using Customers.Integrations.People.UpdatePerson;
using Integrations.People.CreatePerson;

namespace MFFVP.Api.Application.Customers
{
    public interface ICustomersService
    {
        Task<Result<IReadOnlyCollection<PersonResponse>>> GetPersonsAsync(ISender sender);
        Task<Result> CreatePersonAsync(CreatePersonRequestCommand request, ISender sender);
        Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender);
    }
}
