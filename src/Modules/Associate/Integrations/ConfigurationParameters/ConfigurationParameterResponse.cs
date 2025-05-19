using System.Text.Json;
using Associate.Domain.ConfigurationParameters;

namespace Associate.Integrations.ConfigurationParameters;

public sealed record ConfigurationParameterResponse(
    int ConfigurationParameterId,
    Guid Uuid,
    string Name,
    int? ParentId,
    ConfigurationParameter Parent,
    ICollection<ConfigurationParameter> Children,
    bool Status,
    string Type,
    bool Editable,
    bool System,
    JsonDocument Metadata,
    string HomologationCode
);