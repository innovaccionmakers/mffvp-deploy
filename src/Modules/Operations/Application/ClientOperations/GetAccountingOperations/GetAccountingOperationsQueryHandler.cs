using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Operations.Application.ClientOperations.GetAccountingOperations
{
    internal class GetAccountingOperationsQueryHandler(
        IClientOperationRepository repository)
        : IQueryHandler<GetAccountingOperationsQuery, IReadOnlyCollection<GetAccountingOperationsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> Handle(GetAccountingOperationsQuery request, CancellationToken cancellationToken)
        {
            var utcProcessDate = request.ProcessDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.ProcessDate, DateTimeKind.Utc)
                                : request.ProcessDate.ToUniversalTime();

            var clientOperations = await repository.GetAccountingOperationsAsync(request.PortfolioId, utcProcessDate, cancellationToken);

            var response = clientOperations
            .Select(c => new GetAccountingOperationsResponse(
                c.PortfolioId,
                c.AffiliateId,
                c.Amount,
                c.OperationType.Name,
                c.OperationType.Nature,
                c.OperationTypeId,
                c.AuxiliaryInformation.CollectionAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingOperationsResponse>>(response);
        }
    }
}
