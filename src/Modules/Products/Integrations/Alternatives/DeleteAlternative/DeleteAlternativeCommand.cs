using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Alternatives.DeleteAlternative;

public sealed record DeleteAlternativeCommand(
    long AlternativeId
) : ICommand;