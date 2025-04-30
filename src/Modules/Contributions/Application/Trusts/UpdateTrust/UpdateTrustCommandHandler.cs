using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.Trusts;
using Contributions.Integrations.Trusts.UpdateTrust;
using Contributions.Integrations.Trusts;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.Trusts;
internal sealed class UpdateTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustCommand, TrustResponse>
{
    public async Task<Result<TrustResponse>> Handle(UpdateTrustCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await trustRepository.GetAsync(request.TrustId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<TrustResponse>(TrustErrors.NotFound(request.TrustId));
        }

        entity.UpdateDetails(
            request.NewAffiliateId, 
            request.NewObjectiveId, 
            request.NewPortfolioId, 
            request.NewTotalBalance, 
            request.NewTotalUnits, 
            request.NewPrincipal, 
            request.NewEarnings, 
            request.NewTaxCondition, 
            request.NewContingentWithholding, 
            request.NewEarningsWithholding, 
            request.NewAvailableBalance
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustResponse(entity.TrustId, entity.AffiliateId, entity.ObjectiveId, entity.PortfolioId, entity.TotalBalance, entity.TotalUnits, entity.Principal, entity.Earnings, entity.TaxCondition, entity.ContingentWithholding, entity.EarningsWithholding, entity.AvailableBalance);
    }
}