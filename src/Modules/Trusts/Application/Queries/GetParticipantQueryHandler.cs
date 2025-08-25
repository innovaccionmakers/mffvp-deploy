using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;
using Trusts.Integrations.Trusts.Queries;

namespace Trusts.Application.Queries;

internal class GetParticipantQueryHandler(ITrustRepository trustRepository) : IQueryHandler<GetParticipantQuery, int>
{
    public async Task<Result<int>> Handle(GetParticipantQuery request, CancellationToken cancellationToken)
    {
        var result = await trustRepository.GetParticipantAsync(request.TrustIds, cancellationToken);

        return Result.Success(result);
    }
}
