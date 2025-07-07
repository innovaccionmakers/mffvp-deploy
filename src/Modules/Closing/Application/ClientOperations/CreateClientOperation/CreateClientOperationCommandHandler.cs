using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Closing.Application.Abstractions.Data;
using Closing.Domain.ClientOperations;
using Closing.Integrations.ClientOperations.CreateClientOperation;

namespace Closing.Application.ClientOperations.CreateClientOperation;

internal sealed class CreateClientOperationCommandHandler(
    IClientOperationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateClientOperationCommand, ClientOperationResponse>
{
    public async Task<Result<ClientOperationResponse>> Handle(CreateClientOperationCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var result = ClientOperation.Create(
            request.ClientOperationId,
            request.FilingDate,
            request.AffiliateId,
            request.ObjectiveId,
            request.PortfolioId,
            request.Amount,
            request.ProcessDate,
            request.TransactionSubtypeId,
            request.ApplicationDate);

        if (result.IsFailure)
            return Result.Failure<ClientOperationResponse>(result.Error!);

        var clientOperation = result.Value;
        repository.Insert(clientOperation);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ClientOperationResponse(
            clientOperation.ClientOperationId,
            clientOperation.FilingDate,
            clientOperation.AffiliateId,
            clientOperation.ObjectiveId,
            clientOperation.PortfolioId,
            clientOperation.Amount,
            clientOperation.ProcessDate,
            clientOperation.TransactionSubtypeId,
            clientOperation.ApplicationDate);
    }
} 