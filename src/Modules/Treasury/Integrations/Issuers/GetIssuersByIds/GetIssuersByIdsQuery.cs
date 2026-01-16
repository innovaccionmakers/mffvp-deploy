using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.Issuers;

namespace Treasury.Integrations.Issuers.GetIssuersByIds;

public sealed record class GetIssuersByIdsQuery(
    IEnumerable<long> Ids
) : IQuery<IReadOnlyCollection<Issuer>>;

