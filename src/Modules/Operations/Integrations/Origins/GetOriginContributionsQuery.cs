using Common.SharedKernel.Application.Messaging;
using Operations.Domain.Origins;
namespace Operations.Integrations.Origins;

public sealed record class GetOriginContributionsQuery() : IQuery<IReadOnlyCollection<Origin>>;
