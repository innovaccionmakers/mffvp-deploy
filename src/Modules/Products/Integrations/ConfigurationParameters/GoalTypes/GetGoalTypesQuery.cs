using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.GoalTypes;

public sealed record class GetGoalTypesQuery : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;
