using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.Countries.CreateCountry;
public sealed record CreateCountryCommand(
    string Name,
    string ShortName,
    string DaneCode,
    string StandardCode
) : ICommand<CountryResponse>;