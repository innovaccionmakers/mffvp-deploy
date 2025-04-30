using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Integrations.ClientOperations.GetClientOperations;
using Contributions.Integrations.ClientOperations;
using System.Collections.Generic;
using System.Linq;

namespace Contributions.Application.ClientOperations.GetClientOperations;

internal sealed class GetClientOperationsQueryHandler(
    IClientOperationRepository clientoperationRepository)
    : IQueryHandler<GetClientOperationsQuery, IReadOnlyCollection<ClientOperationResponse>>
{
    public async Task<Result<IReadOnlyCollection<ClientOperationResponse>>> Handle(GetClientOperationsQuery request, CancellationToken cancellationToken)
    {
        var entities = await clientoperationRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new ClientOperationResponse(
                e.ClientOperationId,
                e.Date,
                e.AffiliateId,
                e.ObjectiveId,
                e.PortfolioId,
                e.TransactionTypeId,
                e.SubTransactionTypeId,
                e.Amount))
            .ToList();

        return Result.Success<IReadOnlyCollection<ClientOperationResponse>>(response);
    }
}