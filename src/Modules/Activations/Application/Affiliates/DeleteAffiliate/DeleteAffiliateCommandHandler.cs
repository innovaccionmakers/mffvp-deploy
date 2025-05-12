using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Activations.Domain.Affiliates;
using Activations.Integrations.Affiliates.DeleteAffiliate;
using Activations.Application.Abstractions.Data;

namespace Activations.Application.Affiliates.DeleteAffiliate;

internal sealed class DeleteAffiliateCommandHandler(
    IAffiliateRepository affiliateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAffiliateCommand>
{
    public async Task<Result> Handle(DeleteAffiliateCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var affiliate = await affiliateRepository.GetAsync(request.AffiliateId, cancellationToken);
        if (affiliate is null)
        {
            return Result.Failure(AffiliateErrors.NotFound(request.AffiliateId));
        }
        
        affiliateRepository.Delete(affiliate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}