using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

namespace Trusts.Application.CustomerDeals.UpdateCustomerDeal;

internal sealed class UpdateCustomerDealCommandHandler(
    ICustomerDealRepository customerdealRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCustomerDealCommand, CustomerDealResponse>
{
    public async Task<Result<CustomerDealResponse>> Handle(UpdateCustomerDealCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await customerdealRepository.GetAsync(request.CustomerDealId, cancellationToken);
        if (entity is null)
            return Result.Failure<CustomerDealResponse>(CustomerDealErrors.NotFound(request.CustomerDealId));

        entity.UpdateDetails(
            request.NewDate,
            request.NewAffiliateId,
            request.NewObjectiveId,
            request.NewPortfolioId,
            request.NewConfigurationParamId,
            request.NewAmount
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CustomerDealResponse(entity.CustomerDealId, entity.Date, entity.AffiliateId, entity.ObjectiveId,
            entity.PortfolioId, entity.ConfigurationParamId, entity.Amount);
    }
}