using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.Issuers;

namespace Treasury.Integrations.Issuers.GetIssuers;

public sealed record class GetIssuersQuery : IQuery<IReadOnlyCollection<Issuer>>;
