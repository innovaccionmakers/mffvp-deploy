using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts;
using Trusts.Integrations.Trusts.CreateTrust;

namespace Trusts.Application.Trusts.CreateTrust;

internal sealed class CreateTrustCommandHandler(
    ITrustRepository trustRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTrustCommand, TrustResponse>
{
    public async Task<Result<TrustResponse>> Handle(CreateTrustCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        var result = Trust.Create(
            request.AffiliateId,
            request.ClientId,
            request.ObjectiveId,
            request.PortfolioId,
            request.TotalBalance,
            request.TotalUnits,
            request.Principal,
            request.Earnings,
            request.TaxCondition,
            request.ContingentWithholding
        );

        if (result.IsFailure) return Result.Failure<TrustResponse>(result.Error);

        var trust = result.Value;

        trustRepository.Insert(trust);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustResponse(
            trust.TrustId,
            trust.AffiliateId,
            trust.ClientId,
            trust.ObjectiveId,
            trust.PortfolioId,
            trust.TotalBalance,
            trust.TotalUnits,
            trust.Principal,
            trust.Earnings,
            trust.TaxCondition,
            trust.ContingentWithholding
        );
    }
}