using Common.SharedKernel.Application.Messaging;
using Operations.Domain.ConfigurationParameters;

namespace Operations.Integrations.ConfigurationParameters;

public sealed record class GetOriginModesQuery : IQuery<IReadOnlyCollection<OriginMode>>;
