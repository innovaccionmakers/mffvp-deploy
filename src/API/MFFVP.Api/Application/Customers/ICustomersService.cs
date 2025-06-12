using Common.SharedKernel.Domain;
using MediatR;

using Customers.Integrations.People;
using Customers.Integrations.People.UpdatePerson;

namespace MFFVP.Api.Application.Customers
{
    public interface ICustomersService
    {
        Task<Result> UpdatePersonAsync(UpdatePersonCommand request, ISender sender);
    }
}
