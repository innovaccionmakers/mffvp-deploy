using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Integrations.CustomerDeals.DeleteCustomerDeal;

namespace Trusts.Application.CustomerDeals.DeleteCustomerDeal;

internal sealed class DeleteCustomerDealCommandHandler(
    ICustomerDealRepository customerdealRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCustomerDealCommand>
{
    public async Task<Result> Handle(DeleteCustomerDealCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var customerdeal = await customerdealRepository.GetAsync(request.CustomerDealId, cancellationToken);
        if (customerdeal is null) return Result.Failure(CustomerDealErrors.NotFound(request.CustomerDealId));

        customerdealRepository.Delete(customerdeal);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}