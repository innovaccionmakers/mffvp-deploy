using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Operations.Domain.ClientOperations;
using Operations.Domain.OperationTypes;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.Application.ClientOperations.GetAccountingOperations
{
    internal class GetAccountingOperationsQueryHandler(
        IClientOperationRepository repository,
        IOperationTypeRepository operationTypeRepository)
        : IQueryHandler<GetAccountingOperationsQuery, IReadOnlyCollection<GetAccountingOperationsResponse>>
    {        
        public async Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> Handle(GetAccountingOperationsQuery request, CancellationToken cancellationToken)
        {
            var utcProcessDate = request.ProcessDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.ProcessDate, DateTimeKind.Utc)
                                : request.ProcessDate.ToUniversalTime();

            var operationType = await operationTypeRepository.GetByNameAsync(OperationTypeAttributes.Names.Contribution, cancellationToken);
            var listOperationType = operationType.Select(c => (c.Name, c.Nature)).FirstOrDefault();

            var clientOperations = await repository.GetAccountingOperationsAsync(request.PortfolioId, utcProcessDate, cancellationToken);

            var response = clientOperations
            .Select(c => new GetAccountingOperationsResponse(
                c.PortfolioId,
                c.AffiliateId,
                c.Amount,
                listOperationType.Name,
                listOperationType.Nature,
                c.OperationTypeId,
                c.CollectionAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingOperationsResponse>>(response);
        }
    }
}
