using Closing.Application.Abstractions.Data;
using Closing.Domain.ClientOperations;
using Closing.Integrations.ClientOperations.CreateClientOperation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.ClientOperations.CreateClientOperation;

internal sealed class CreateClientOperationCommandHandler(
    IClientOperationRepository repository,
    ILogger<CreateClientOperationCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateClientOperationCommand, ClientOperationResponse>
{
    private const string ClassName = nameof(CreateClientOperationCommandHandler);
    public async Task<Result<ClientOperationResponse>> Handle(CreateClientOperationCommand request, CancellationToken cancellationToken)
    {
        //await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var existing = await repository.GetForUpdateByIdAsync(request.ClientOperationId, cancellationToken);

        if (existing is null)
        {


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
            {
                logger.LogWarning("{Class} - Falló creación de ClientOperation. Error: {Error}", ClassName, result.Error);
                return Result.Failure<ClientOperationResponse>(result.Error!);
            }


            var clientOperation = result.Value;
            repository.Insert(clientOperation);
            logger.LogInformation("{Class} - Closing ClientOperation creada e insertada: {@ClientOperation}", ClassName, clientOperation.ClientOperationId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{Class} - Cambios guardados en base de datos", ClassName);

            //await transaction.CommitAsync(cancellationToken);

            return MapToResponse(clientOperation);
        }
    
        else
        {
            existing.UpdateDetails(
                request.FilingDate,
                request.AffiliateId,
                request.ObjectiveId,
                request.PortfolioId,
                request.Amount,
                request.ProcessDate,
                request.TransactionSubtypeId,
                request.ApplicationDate);

            repository.Update(existing);
            logger.LogInformation("{Class} - Update ClientOperation {@Entity}", ClassName, existing.ClientOperationId);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            //await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("{Class} - Commit Update para ClientOperationId {Id}", ClassName, existing.ClientOperationId);

            return MapToResponse(existing);
        }

    }
    private static ClientOperationResponse MapToResponse(ClientOperation entity) =>
      new(
          entity.ClientOperationId,
          entity.FilingDate,
          entity.AffiliateId,
          entity.ObjectiveId,
          entity.PortfolioId,
          entity.Amount,
          entity.ProcessDate,
          entity.OperationTypeId,
          entity.ApplicationDate);
}