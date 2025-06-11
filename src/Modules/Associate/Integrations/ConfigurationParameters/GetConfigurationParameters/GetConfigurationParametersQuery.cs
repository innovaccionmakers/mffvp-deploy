using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Associate.Integrations.ConfigurationParameters.GetConfigurationParameters;
public sealed record GetConfigurationParametersQuery(IEnumerable<Guid>? Uuids = null) : IQuery<IReadOnlyCollection<ConfigurationParameterResponse>>;