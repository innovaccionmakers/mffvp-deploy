using Common.SharedKernel.Application.Messaging;
using System;

namespace Associate.Integrations.ConfigurationParameters.DeleteConfigurationParameter;
public sealed record DeleteConfigurationParameterCommand(
    int ConfigurationParameterId
) : ICommand;