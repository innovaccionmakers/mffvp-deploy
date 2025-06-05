using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.UpdateActivate;

public sealed record UpdateActivateCommand(
    [property: JsonPropertyName("TipoId")]
    string IdentificationType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    bool Pensioner
) : ICommand;