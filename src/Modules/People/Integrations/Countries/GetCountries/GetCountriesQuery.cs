using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace People.Integrations.Countries.GetCountries;
public sealed record GetCountriesQuery() : IQuery<IReadOnlyCollection<CountryResponse>>;