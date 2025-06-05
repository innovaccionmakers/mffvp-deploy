using Common.SharedKernel.Application.Messaging;
using System;
using System.Text.Json.Serialization;

namespace Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
public sealed record UpdatePensionRequirementCommand(
    [property: JsonPropertyName("TipoId")]
    string IdentificationType,
    
    [property: JsonPropertyName("Identificacion")]
    string Identification,
    
    [property: JsonPropertyName("IdRequisitoPension")]
    int PensionRequirementId,
    
    [property: JsonPropertyName("Estado")]
    bool Status
) : ICommand;