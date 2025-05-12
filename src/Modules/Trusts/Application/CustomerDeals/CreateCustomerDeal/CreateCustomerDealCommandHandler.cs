using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.CreateCustomerDeal;

namespace Trusts.Application.CustomerDeals.CreateCustomerDeal;

internal sealed class CreateCustomerDealCommandHandler(
    ICustomerDealRepository customerdealRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCustomerDealCommand, CustomerDealResponse>
{
    public async Task<Result<CustomerDealResponse>> Handle(CreateCustomerDealCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        var result = CustomerDeal.Create(
            request.Date,
            request.AffiliateId,
            request.ObjectiveId,
            request.PortfolioId,
            request.ConfigurationParamId,
            request.Amount
        );

        if (result.IsFailure) return Result.Failure<CustomerDealResponse>(result.Error);

        var customerdeal = result.Value;

        customerdealRepository.Insert(customerdeal);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CustomerDealResponse(
            customerdeal.CustomerDealId,
            customerdeal.Date,
            customerdeal.AffiliateId,
            customerdeal.ObjectiveId,
            customerdeal.PortfolioId,
            customerdeal.ConfigurationParamId,
            customerdeal.Amount
        );
    }
}