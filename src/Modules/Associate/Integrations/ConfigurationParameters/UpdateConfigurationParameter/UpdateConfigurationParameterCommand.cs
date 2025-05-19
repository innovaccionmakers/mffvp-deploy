using Associate.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json;

namespace Associate.Integrations.ConfigurationParameters.UpdateConfigurationParameter;
public sealed record UpdateConfigurationParameterCommand(
    int ConfigurationParameterId,
    Guid NewUuid,
    string NewName,
    int? NewParentId,
    ConfigurationParameter NewParent,
    ICollection<ConfigurationParameter> NewChildren,
    bool NewStatus,
    string NewType,
    bool NewEditable,
    bool NewSystem,
    JsonDocument NewMetadata,
    string NewHomologationCode
) : ICommand<ConfigurationParameterResponse>;