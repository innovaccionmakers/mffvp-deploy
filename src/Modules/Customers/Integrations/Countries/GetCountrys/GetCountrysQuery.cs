using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Customers.Integrations.Countries.GetCountrys;
public sealed record GetCountrysQuery() : IQuery<IReadOnlyCollection<CountryResponse>>;