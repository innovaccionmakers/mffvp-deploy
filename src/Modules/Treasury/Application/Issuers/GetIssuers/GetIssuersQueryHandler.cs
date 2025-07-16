using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.Issuers;
using Treasury.Integrations.Issuers.GetIssuers;

namespace Treasury.Application.Issuers.GetIssuers;

public class GetIssuersQueryHandler(
    IIssuerRepository issuerRepository) : IQueryHandler<GetIssuersQuery, IReadOnlyCollection<Issuer>>
{
    public async Task<Result<IReadOnlyCollection<Issuer>>> Handle(GetIssuersQuery request, CancellationToken cancellationToken)
    {
        var result = await issuerRepository.GetAllAsync(cancellationToken);
        return Result.Success(result);
    }
}