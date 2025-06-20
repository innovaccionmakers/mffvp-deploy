using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ConfigurationParameters;

public sealed record class GetCollectionMethodsQuery: IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;