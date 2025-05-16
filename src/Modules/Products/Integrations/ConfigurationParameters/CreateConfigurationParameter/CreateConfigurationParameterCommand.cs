using System.Text.Json;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.CreateConfigurationParameter;

public sealed record CreateConfigurationParameterCommand(
    string Name,
    string HomologationCode,
    string Type = "item",
    int? ParentId = null,
    bool Status = true,
    bool Editable = true,
    bool System = false,
    JsonDocument? Metadata = null
) : ICommand<ConfigurationParameterResponse>;