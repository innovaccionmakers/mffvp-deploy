using Common.SharedKernel.Application.Messaging;
using System;

namespace Associate.Integrations.ConfigurationParameters.GetConfigurationParameter;
public sealed record GetConfigurationParameterQuery(
    int ConfigurationParameterId
) : IQuery<ConfigurationParameterResponse>;