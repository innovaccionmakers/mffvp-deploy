using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.Countries.GetCountry;

public sealed record GetCountryQuery(
    int CountryId
) : IQuery<CountryResponse>;