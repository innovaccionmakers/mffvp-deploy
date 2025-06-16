using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Customers.Integrations.Municipalities.GetMunicipalitys;
public sealed record GetMunicipalitysQuery() : IQuery<IReadOnlyCollection<MunicipalityResponse>>;