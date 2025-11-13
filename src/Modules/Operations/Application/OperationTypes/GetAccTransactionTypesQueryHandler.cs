using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;

namespace Operations.Application.OperationTypes
{
    internal class GetAccTransactionTypesQueryHandler(IOperationTypeRepository repository) : IQueryHandler<GetAccTransactionTypesQuery, IReadOnlyCollection<OperationType>>
    {
        public async Task<Result<IReadOnlyCollection<OperationType>>> Handle(GetAccTransactionTypesQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetAccTransactionTypesAsync(cancellationToken);
            return Result.Success(result);
        }
    }
}
