using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.Trusts;
using Contributions.Integrations.Trusts.CreateTrust;
using Contributions.Integrations.Trusts;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.Trusts.CreateTrust

{
    internal sealed class CreateTrustCommandHandler(
        ITrustRepository trustRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateTrustCommand, TrustResponse>
    {
        public async Task<Result<TrustResponse>> Handle(CreateTrustCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = Trust.Create(
                request.AffiliateId,
                request.ObjectiveId,
                request.PortfolioId,
                request.TotalBalance,
                request.TotalUnits,
                request.Principal,
                request.Earnings,
                request.TaxCondition,
                request.ContingentWithholding,
                request.EarningsWithholding,
                request.AvailableBalance
            );

            if (result.IsFailure)
            {
                return Result.Failure<TrustResponse>(result.Error);
            }

            var trust = result.Value;
            
            trustRepository.Insert(trust);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new TrustResponse(
                trust.TrustId,
                trust.AffiliateId,
                trust.ObjectiveId,
                trust.PortfolioId,
                trust.TotalBalance,
                trust.TotalUnits,
                trust.Principal,
                trust.Earnings,
                trust.TaxCondition,
                trust.ContingentWithholding,
                trust.EarningsWithholding,
                trust.AvailableBalance
            );
        }
    }
}