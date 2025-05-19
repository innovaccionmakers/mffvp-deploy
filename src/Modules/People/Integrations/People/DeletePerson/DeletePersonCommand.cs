using Common.SharedKernel.Application.Messaging;
using System;

namespace People.Integrations.People.DeletePerson;

public sealed record DeletePersonCommand(
    long PersonId
) : ICommand;