using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.Issuers;
using Treasury.Integrations.Issuers.GetIssuersByIds;

namespace Treasury.Application.Issuers.GetIssuersByIds;

public class GetIssuersByIdsQueryHandler(
    IIssuerRepository issuerRepository) : IQueryHandler<GetIssuersByIdsQuery, IReadOnlyCollection<Issuer>>
{
    public async Task<Result<IReadOnlyCollection<Issuer>>> Handle(GetIssuersByIdsQuery request, CancellationToken cancellationToken)
    {
        var result = await issuerRepository.GetByIdsAsync(request.Ids, cancellationToken);
        return Result.Success(result);
    }
}

