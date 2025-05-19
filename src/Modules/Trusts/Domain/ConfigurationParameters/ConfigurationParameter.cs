using System.Text.Json;
using Common.SharedKernel.Domain;

namespace Trusts.Domain.ConfigurationParameters;

public sealed class ConfigurationParameter : Entity
{
    private ConfigurationParameter()
    {
    }

    public int ConfigurationParameterId { get; private set; }
    public Guid Uuid { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public int? ParentId { get; private set; }
    public ConfigurationParameter Parent { get; private set; }
    public ICollection<ConfigurationParameter> Children { get; private set; } = new List<ConfigurationParameter>();
    public bool Status { get; private set; } = true;
    public string Type { get; private set; } = "category";
    public bool Editable { get; private set; } = true;
    public bool System { get; private set; }
    public JsonDocument Metadata { get; private set; } = JsonDocument.Parse("{}");
    public string HomologationCode { get; private set; }

    public static ConfigurationParameter Create(
        string name,
        string homologationCode,
        string type = "item",
        int? parentId = null,
        bool status = true,
        bool editable = true,
        bool system = false,
        JsonDocument metadata = null!)
    {
        return new ConfigurationParameter
        {
            Name = name,
            HomologationCode = homologationCode,
            Type = type,
            ParentId = parentId,
            Status = status,
            Editable = editable,
            System = system,
            Metadata = metadata ?? JsonDocument.Parse("{}")
        };
    }

    public void UpdateDetails(
        string newName,
        string newHomologationCode,
        string newType,
        int? newParentId,
        bool newStatus,
        bool newEditable,
        bool newSystem,
        JsonDocument newMetadata
    )
    {
        Name = newName;
        HomologationCode = newHomologationCode;
        Type = newType;
        ParentId = newParentId;
        Status = newStatus;
        Editable = newEditable;
        System = newSystem;
        Metadata = newMetadata;
    }
}