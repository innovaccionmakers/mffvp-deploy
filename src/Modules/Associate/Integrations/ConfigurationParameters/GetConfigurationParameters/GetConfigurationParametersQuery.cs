using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Associate.Integrations.ConfigurationParameters.GetConfigurationParameters;
public sealed record GetConfigurationParametersQuery() : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;