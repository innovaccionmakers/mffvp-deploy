using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.GetConfigurationParameter;

public sealed record GetConfigurationParameterQuery(
    int ConfigurationParameterId
) : IQuery<ConfigurationParameterResponse>;