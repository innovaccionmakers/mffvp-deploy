using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.People.GetPerson;

public sealed record GetPersonQuery(
    long PersonId
) : IQuery<PersonResponse>;