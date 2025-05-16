using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.DeleteConfigurationParameter;

public sealed record DeleteConfigurationParameterCommand(
    int ConfigurationParameterId
) : ICommand;