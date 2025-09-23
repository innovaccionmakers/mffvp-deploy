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
                request.ApplicationDate,
                request.Status,
                request.TrustId,
                request.LinkedClientOperationId,
                request.Units);

            if (result.IsFailure)
            {
                logger.LogError("{Class} - Error creating ClientOperation {Id}: {Code} - {Description}", ClassName, request.ClientOperationId, result.Error.Code, result.Error.Description);
                return Result.Failure<ClientOperationResponse>(result.Error);
            }

            var clientOperation = result.Value;
            repository.Insert(clientOperation);
            logger.LogInformation("{Class} - Closing ClientOperation creada e insertada: {@ClientOperation}", ClassName, clientOperation.ClientOperationId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{Class} - Cambios guardados en base de datos", ClassName);

            return MapToResponse(clientOperation);
        }

        existing.UpdateDetails(
            request.FilingDate,
            request.AffiliateId,
            request.ObjectiveId,
            request.PortfolioId,
            request.Amount,
            request.ProcessDate,
            request.TransactionSubtypeId,
            request.ApplicationDate,
            request.Status,
            request.TrustId,
            request.LinkedClientOperationId,
            request.Units);

        repository.Update(existing);
        logger.LogInformation("{Class} - Update ClientOperation {@Entity}", ClassName, existing.ClientOperationId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Class} - Commit Update para ClientOperationId {Id}", ClassName, existing.ClientOperationId);

        return MapToResponse(existing);
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
            entity.ApplicationDate,
            entity.Status,
            entity.TrustId,
            entity.LinkedClientOperationId,
            entity.Units);
}
