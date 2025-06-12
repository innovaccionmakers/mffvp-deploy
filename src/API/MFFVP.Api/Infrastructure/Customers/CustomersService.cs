
using Common.SharedKernel.Domain;
using MediatR;

using Customers.Integrations.People.UpdatePerson;

using MFFVP.Api.Application.Customers;

namespace MFFVP.Api.Infrastructure.Customers
{
    public sealed class CustomersService : ICustomersService
    {
        public async Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender)
        {
            return await sender.Send(request);
        }
    }
}