using Common.SharedKernel.Application.Messaging;
using System;
using System.Text.Json;
using Associate.Domain.ConfigurationParameters;

namespace Associate.Integrations.ConfigurationParameters.CreateConfigurationParameter;
public sealed record CreateConfigurationParameterCommand(
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
) : ICommand<ConfigurationParameterResponse>;