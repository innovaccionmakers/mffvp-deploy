using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace People.Integrations.People.GetPeople;
public sealed record GetPeopleQuery() : IQuery<IReadOnlyCollection<PersonResponse>>;