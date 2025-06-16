using Common.SharedKernel.Application.Messaging;
using System;

namespace Customers.Integrations.Countries.GetCountry;
public sealed record GetCountryQuery(
    string HomologatedCode
) : IQuery<CountryResponse>;