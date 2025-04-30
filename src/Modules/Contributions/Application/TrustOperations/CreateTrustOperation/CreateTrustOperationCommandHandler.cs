using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.TrustOperations;
using Contributions.Integrations.TrustOperations.CreateTrustOperation;
using Contributions.Integrations.TrustOperations;
using Contributions.Application.Abstractions.Data;
using Contributions.Domain.ClientOperations;using Contributions.Domain.Trusts;
namespace Contributions.Application.TrustOperations.CreateTrustOperation

{
    internal sealed class CreateTrustOperationCommandHandler(
        IClientOperationRepository clientoperationRepository,
        ITrustRepository trustRepository,
        ITrustOperationRepository trustoperationRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateTrustOperationCommand, TrustOperationResponse>
    {
        public async Task<Result<TrustOperationResponse>> Handle(CreateTrustOperationCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var clientoperation = await clientoperationRepository.GetAsync(request.ClientOperationId, cancellationToken);
            var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);

            if (clientoperation is null)
                return Result.Failure<TrustOperationResponse>(ClientOperationErrors.NotFound(request.ClientOperationId));
            if (trust is null)
                return Result.Failure<TrustOperationResponse>(TrustErrors.NotFound(request.TrustId));


            var result = TrustOperation.Create(
                request.Amount,
                clientoperation,
                trust
            );

            if (result.IsFailure)
            {
                return Result.Failure<TrustOperationResponse>(result.Error);
            }

            var trustoperation = result.Value;
            
            trustoperationRepository.Insert(trustoperation);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new TrustOperationResponse(
                trustoperation.TrustOperationId,
                trustoperation.ClientOperationId,
                trustoperation.TrustId,
                trustoperation.Amount
            );
        }
    }
}