
using Common.SharedKernel.Domain;
using MediatR;

using Customers.Integrations.People.UpdatePerson;

using MFFVP.Api.Application.Customers;
using Customers.Integrations.People.CreatePerson;
using Customers.Integrations.People;
using Customers.Integrations.People.GetPersons;
using Integrations.People.CreatePerson;

namespace MFFVP.Api.Infrastructure.Customers
{
    public sealed class CustomersService : ICustomersService
    {
        public async Task<Result> CreatePersonAsync(CreatePersonRequestCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result<IReadOnlyCollection<PersonResponse>>> GetPersonsAsync(ISender sender)
        {
            return await sender.Send(new GetPersonsQuery());
        }

        public async Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender)
        {
            return await sender.Send(request);
        }
    }
}