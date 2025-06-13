using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Activates.UpdateActivate;

public sealed record UpdateActivateCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    string DocumentType,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("Pensionado")]
    bool Pensioner
) : ICommand;