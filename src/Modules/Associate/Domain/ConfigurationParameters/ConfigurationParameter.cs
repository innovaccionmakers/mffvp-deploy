using System.Text.Json;
using Common.SharedKernel.Domain;

namespace Associate.Domain.ConfigurationParameters;
public sealed class ConfigurationParameter : Entity
{
    public int ConfigurationParameterId { get; private set; }
    public Guid Uuid { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public int? ParentId { get; private set; }
    public ConfigurationParameter Parent { get; private set; }
    public ICollection<ConfigurationParameter> Children { get; private set; } = new List<ConfigurationParameter>();
    public bool Status { get; private set; } = true;
    public string Type { get; private set; } = "category";
    public bool Editable { get; private set; } = true;
    public bool System { get; private set; } = false;
    public JsonDocument Metadata { get; private set; } = JsonDocument.Parse("{}");
    public string HomologationCode { get; private set; }

    private ConfigurationParameter() { }

    public static Result<ConfigurationParameter> Create(
        Guid uuid, string name, int? parentid, ConfigurationParameter parent, ICollection<ConfigurationParameter> children, bool status, string type, bool editable, bool system, JsonDocument metadata, string homologationcode
    )
    {
        var configurationparameter = new ConfigurationParameter
        {
                ConfigurationParameterId = new int(),
                Uuid = uuid,
                Name = name,
                ParentId = parentid,
                Parent = parent,
                Children = children,
                Status = status,
                Type = type,
                Editable = editable,
                System = system,
                Metadata = metadata,
                HomologationCode = homologationcode,
        };
        configurationparameter.Raise(new ConfigurationParameterCreatedDomainEvent(configurationparameter.ConfigurationParameterId));
        return Result.Success(configurationparameter);
    }

    public void UpdateDetails(
        Guid newUuid, string newName, int? newParentId, ConfigurationParameter newParent, ICollection<ConfigurationParameter> newChildren, bool newStatus, string newType, bool newEditable, bool newSystem, JsonDocument newMetadata, string newHomologationCode
    )
    {
        Uuid = newUuid;
        Name = newName;
        ParentId = newParentId;
        Parent = newParent;
        Children = newChildren;
        Status = newStatus;
        Type = newType;
        Editable = newEditable;
        System = newSystem;
        Metadata = newMetadata;
        HomologationCode = newHomologationCode;
    }
}