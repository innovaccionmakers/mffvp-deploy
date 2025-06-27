using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ConfigurationParameters;

public sealed record class GetOriginModesQuery(
    int OriginId
) : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;
