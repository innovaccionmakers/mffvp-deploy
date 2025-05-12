using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.CreateCustomerDeal;
using Trusts.Integrations.CustomerDeals.DeleteCustomerDeal;
using Trusts.Integrations.CustomerDeals.GetCustomerDeal;
using Trusts.Integrations.CustomerDeals.GetCustomerDeals;
using Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

namespace MFFVP.Api.Infrastructure.Trusts;

public sealed class CustomerDealsService : ICustomerDealsService
{
    public async Task<Result<IReadOnlyCollection<CustomerDealResponse>>> GetCustomerDealsAsync(ISender sender)
    {
        return await sender.Send(new GetCustomerDealsQuery());
    }

    public async Task<Result<CustomerDealResponse>> GetCustomerDealAsync(Guid id, ISender sender)
    {
        return await sender.Send(new GetCustomerDealQuery(id));
    }

    public async Task<Result> CreateCustomerDealAsync(CreateCustomerDealCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> UpdateCustomerDealAsync(UpdateCustomerDealCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> DeleteCustomerDealAsync(Guid id, ISender sender)
    {
        return await sender.Send(new DeleteCustomerDealCommand(id));
    }
}