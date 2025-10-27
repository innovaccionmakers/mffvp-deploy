using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ConfigurationParameters;

public sealed record class GetCancellationClauseQuery
    : IQuery<IReadOnlyCollection<CancellationClauseConfigurationParameterResponse>>;
