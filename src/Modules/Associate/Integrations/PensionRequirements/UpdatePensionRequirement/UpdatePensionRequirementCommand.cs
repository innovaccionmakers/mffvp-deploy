using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using System.Text.Json.Serialization;

namespace Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
public sealed record UpdatePensionRequirementCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string DocumentType,
    
    [property: JsonPropertyName("Identificacion")]
    [property: JsonConverter(typeof(EmptyStringToNullStringConverter))]
    string Identification,

    [property: JsonPropertyName("IdRequisitoPension")]
    [property: JsonConverter(typeof(NullableIntConverter))]
    int? PensionRequirementId,
    
    [property: JsonPropertyName("Estado")]    
    [property: JsonConverter(typeof(BooleanOrStringToBooleanConverter))]
    bool? Status
) : ICommand;