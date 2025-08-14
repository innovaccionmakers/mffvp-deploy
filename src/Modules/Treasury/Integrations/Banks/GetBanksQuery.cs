using Common.SharedKernel.Application.Messaging;
using Treasury.Domain.Issuers;

namespace Treasury.Integrations.Banks;

public sealed record class GetBanksQuery : IQuery<IReadOnlyCollection<Issuer>>;
