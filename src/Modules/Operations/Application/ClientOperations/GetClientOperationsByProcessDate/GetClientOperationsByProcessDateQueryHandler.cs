using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;
using Operations.Integrations.ClientOperations;
using Operations.Integrations.ClientOperations.GetClientOperationsByProcessDate;

namespace Operations.Application.ClientOperations.GetClientOperationsByProcessDate
{
    internal class GetClientOperationsByProcessDateQueryHandler(
        IClientOperationRepository repository)
        : IQueryHandler<GetClientOperationsByProcessDateQuery, IEnumerable<GetClientOperationsByProcessDateResponse>>
    {
        public async Task<Result<IEnumerable<GetClientOperationsByProcessDateResponse>>> Handle(GetClientOperationsByProcessDateQuery request, CancellationToken cancellationToken)
        {
            var utcProcessDate = request.ProcessDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(request.ProcessDate, DateTimeKind.Utc)
                                : request.ProcessDate.ToUniversalTime();

            var clientOperations = await repository.GetClientOperationsByProcessDateAsync(utcProcessDate, cancellationToken);

            var response = clientOperations
            .Select(c => new GetClientOperationsByProcessDateResponse(
                c.Amount,
                c.AuxiliaryInformation.CollectionAccount,
                c.AuxiliaryInformation.PaymentMethodDetail,
                c.SubtransactionType.Name))
            .ToList();

            return Result.Success<IEnumerable<GetClientOperationsByProcessDateResponse>>(response);
        }
    }
}
