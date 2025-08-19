using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.Domain.Issuers;
using Treasury.Integrations.Banks;

namespace Treasury.Application.Banks;

public class GetBanksQueryHandler(
    IIssuerRepository issuerRepository) : IQueryHandler<GetBanksQuery, IReadOnlyCollection<Issuer>>
{
    public async Task<Result<IReadOnlyCollection<Issuer>>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        var banks = await issuerRepository.GetBanksAsync(cancellationToken);
        return Result.Success(banks);
    }
}
