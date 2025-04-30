using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Integrations.ClientOperations.CreateClientOperation;
using Contributions.Integrations.ClientOperations;
using Contributions.Application.Abstractions.Data;
using Contributions.Application.Abstractions.Rules;

namespace Contributions.Application.ClientOperations.CreateClientOperation

{
    internal sealed class CreateClientOperationCommandHandler(
        IClientOperationRepository clientoperationRepository,
        IUnitOfWork unitOfWork,
        IRuleEvaluator rules
        )
        : ICommandHandler<CreateClientOperationCommand, ClientOperationResponse>
    {
        private const string Workflow = "Contributions.ClientOperation.Validation";

        public async Task<Result<ClientOperationResponse>> Handle(CreateClientOperationCommand request, CancellationToken cancellationToken)
        {
            var (ok, _, errors) = await rules.EvaluateAsync(Workflow, request, cancellationToken);

            if (!ok)
                return Result.Failure<ClientOperationResponse>(
                    Error.Validation("RuleValidation",
                        string.Join("; ", errors.Select(e => $"{e.Code}: {e.Message}"))));

            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var result = ClientOperation.Create(
                request.Date,
                request.AffiliateId,
                request.ObjectiveId,
                request.PortfolioId,
                request.TransactionTypeId,
                request.SubTransactionTypeId,
                request.Amount
            );

            if (result.IsFailure)
            {
                return Result.Failure<ClientOperationResponse>(result.Error);
            }

            var clientoperation = result.Value;
            
            clientoperationRepository.Insert(clientoperation);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ClientOperationResponse(
                clientoperation.ClientOperationId,
                clientoperation.Date,
                clientoperation.AffiliateId,
                clientoperation.ObjectiveId,
                clientoperation.PortfolioId,
                clientoperation.TransactionTypeId,
                clientoperation.SubTransactionTypeId,
                clientoperation.Amount
            );
        }
    }
}