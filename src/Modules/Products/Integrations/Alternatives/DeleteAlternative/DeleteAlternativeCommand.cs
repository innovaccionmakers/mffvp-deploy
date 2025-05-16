using Common.SharedKernel.Application.Messaging;
using System;

namespace Products.Integrations.Alternatives.DeleteAlternative;
public sealed record DeleteAlternativeCommand(
    long AlternativeId
) : ICommand;