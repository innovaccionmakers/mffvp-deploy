using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations;
using Operations.Integrations.ClientOperations.GetClientOperationsByProcessDate;

namespace Operations.Application.ClientOperations.GetClientOperationsByProcessDate
{
    internal class GetClientOperationsByProcessDateQueryHandler(
        IClientOperationRepository repository)
        : IQueryHandler<GetClientOperationsByProcessDateQuery, IReadOnlyCollection<ClientOperationsByProcessDateResponse>>
    {
        public async Task<Result<IReadOnlyCollection<ClientOperationsByProcessDateResponse>>> Handle(GetClientOperationsByProcessDateQuery request, CancellationToken cancellationToken)
        {
            var utcProcessDate = request.ProcessDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.ProcessDate, DateTimeKind.Utc)
                                : request.ProcessDate.ToUniversalTime();

            var clientOperations = await repository.GetClientOperationsByProcessDateAsync(utcProcessDate, cancellationToken);

            var response = clientOperations
            .Select(c => new ClientOperationsByProcessDateResponse(
                c.Amount,
                c.AuxiliaryInformation.CollectionAccount,
                c.AuxiliaryInformation.PaymentMethodDetail,
                c.OperationType.Name))
            .ToList();

            return Result.Success<IReadOnlyCollection<ClientOperationsByProcessDateResponse>>(response);
        }
    }
}
