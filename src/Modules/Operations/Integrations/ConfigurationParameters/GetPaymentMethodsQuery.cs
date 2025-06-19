using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ConfigurationParameters;

public sealed record class GetPaymentMethodsQuery : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;