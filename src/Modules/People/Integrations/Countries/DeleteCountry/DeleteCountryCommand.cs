using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.Countries.DeleteCountry;

public sealed record DeleteCountryCommand(
    int CountryId
) : ICommand;