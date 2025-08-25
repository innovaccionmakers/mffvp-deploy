using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.Queries;

public sealed record GetParticipantQuery(IEnumerable<long> TrustIds) : IQuery<int>;
