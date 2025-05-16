using System.Text.Json;
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.ConfigurationParameters.UpdateConfigurationParameter;

public sealed record UpdateConfigurationParameterCommand(
    int ConfigurationParameterId,
    string? NewName = null,
    string? NewHomologationCode = null,
    string? NewType = null,
    int? NewParentId = null,
    bool? NewStatus = null,
    bool? NewEditable = null,
    bool? NewSystem = null,
    JsonDocument? NewMetadata = null
) : ICommand<ConfigurationParameterResponse>;