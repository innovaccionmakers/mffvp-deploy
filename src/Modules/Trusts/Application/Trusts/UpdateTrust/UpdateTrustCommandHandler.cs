using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts;
using Trusts.Integrations.Trusts.UpdateTrust;

namespace Trusts.Application.Trusts;

internal sealed class UpdateTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustCommand, TrustResponse>
{
    public async Task<Result<TrustResponse>> Handle(UpdateTrustCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await trustRepository.GetAsync(request.TrustId, cancellationToken);
        if (entity is null) return Result.Failure<TrustResponse>(TrustErrors.NotFound(request.TrustId));

        entity.UpdateDetails(
            request.NewAffiliateId,
            request.NewClientId,
            request.NewCreationDate,
            request.NewObjectiveId,
            request.NewPortfolioId,
            request.NewTotalBalance,
            request.NewTotalUnits,
            request.NewPrincipal,
            request.NewEarnings,
            request.NewTaxCondition,
            request.NewContingentWithholding,
            request.NewEarningsWithholding,
            request.NewAvailableAmount,
            request.NewContingentWithholdingPercentage
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustResponse(entity.TrustId, entity.AffiliateId, entity.ClientId, entity.CreationDate,
            entity.ObjectiveId, entity.PortfolioId, entity.TotalBalance, entity.TotalUnits, entity.Principal,
            entity.Earnings, entity.TaxCondition, entity.ContingentWithholding, entity.EarningsWithholding,
            entity.AvailableAmount, entity.ContingentWithholdingPercentage);
    }
}