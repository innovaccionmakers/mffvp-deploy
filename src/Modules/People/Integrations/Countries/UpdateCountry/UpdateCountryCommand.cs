using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.Countries.UpdateCountry;
public sealed record UpdateCountryCommand(
    int CountryId,
    string NewName,
    string NewShortName,
    string NewDaneCode,
    string NewStandardCode
) : ICommand<CountryResponse>;