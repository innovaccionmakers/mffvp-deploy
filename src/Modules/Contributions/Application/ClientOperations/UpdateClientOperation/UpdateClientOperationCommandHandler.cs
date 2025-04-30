using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Integrations.ClientOperations.UpdateClientOperation;
using Contributions.Integrations.ClientOperations;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.ClientOperations;
internal sealed class UpdateClientOperationCommandHandler(
    IClientOperationRepository clientoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateClientOperationCommand, ClientOperationResponse>
{
    public async Task<Result<ClientOperationResponse>> Handle(UpdateClientOperationCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await clientoperationRepository.GetAsync(request.ClientOperationId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<ClientOperationResponse>(ClientOperationErrors.NotFound(request.ClientOperationId));
        }

        entity.UpdateDetails(
            request.NewDate, 
            request.NewAffiliateId, 
            request.NewObjectiveId, 
            request.NewPortfolioId, 
            request.NewTransactionTypeId, 
            request.NewSubTransactionTypeId, 
            request.NewAmount
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ClientOperationResponse(entity.ClientOperationId, entity.Date, entity.AffiliateId, entity.ObjectiveId, entity.PortfolioId, entity.TransactionTypeId, entity.SubTransactionTypeId, entity.Amount);
    }
}