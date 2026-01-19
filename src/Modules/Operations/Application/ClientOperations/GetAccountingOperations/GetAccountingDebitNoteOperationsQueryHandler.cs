using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.Application.ClientOperations.GetAccountingOperations
{
    internal class GetAccountingDebitNoteOperationsQueryHandler(
        IClientOperationRepository repository,
        IOperationTypeRepository operationTypeRepository)
        : IQueryHandler<GetAccountingDebitNoteOperationsQuery, IReadOnlyCollection<GetAccountingOperationsResponse>>
    {        
        public async Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> Handle(GetAccountingDebitNoteOperationsQuery request, CancellationToken cancellationToken)
        {
            var utcProcessDate = request.ProcessDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.ProcessDate, DateTimeKind.Utc)
                                : request.ProcessDate.ToUniversalTime();

            var operationType = await operationTypeRepository.GetByNameAsync(OperationTypeAttributes.Names.DebitNote, cancellationToken);
            var listOperationType = operationType.Select(c => (c.Name, c.Nature)).FirstOrDefault();

            var clientOperations = await repository.GetAccountingDebitNoteOperationsAsync(request.PortfolioId, utcProcessDate, cancellationToken);

            var response = clientOperations
            .Select(c => new GetAccountingOperationsResponse(
                c.ClientOperationId,
                c.PortfolioId,
                c.AffiliateId,
                c.Amount,
                listOperationType.Name,
                listOperationType.Nature,
                c.OperationTypeId,
                c.CollectionAccount ?? "",
                c.LinkedClientOperationId))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingOperationsResponse>>(response);
        }
    }
}
