using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Customers.Integrations.People.GetPersons;
public sealed record GetPersonsQuery() : IQuery<IReadOnlyCollection<PersonResponse>>;