using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.CreateCustomerDeal;
using Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

namespace MFFVP.Api.Application.Trusts;

public interface ICustomerDealsService
{
    Task<Result<IReadOnlyCollection<CustomerDealResponse>>> GetCustomerDealsAsync(ISender sender);
    Task<Result<CustomerDealResponse>> GetCustomerDealAsync(Guid id, ISender sender);
    Task<Result> CreateCustomerDealAsync(CreateCustomerDealCommand request, ISender sender);
    Task<Result> UpdateCustomerDealAsync(UpdateCustomerDealCommand request, ISender sender);
    Task<Result> DeleteCustomerDealAsync(Guid id, ISender sender);
}