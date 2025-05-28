using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.UpdateActivate;

public sealed record UpdateActivateCommand(
    string IdentificationType,
    string Identification,
    bool Pensioner
) : ICommand;