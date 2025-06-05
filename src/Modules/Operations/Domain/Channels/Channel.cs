using System.Collections.Generic;
using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;

namespace Operations.Domain.Channels;

public sealed class Channel : Entity
{
    public int ChannelId { get; private set; }
    public string Name { get; private set; }
    public string HomologatedCode { get; private set; }
    public bool System { get; private set; }
    public string Status { get; private set; }

    private readonly List<AuxiliaryInformation> _auxiliaryInformations = new();
    public IReadOnlyCollection<AuxiliaryInformation> AuxiliaryInformations => _auxiliaryInformations;

    private Channel()
    {
    }

    public static Result<Channel> Create(
        string name,
        string homologatedCode,
        bool system,
        string status
    )
    {
        var channel = new Channel
        {
            ChannelId = default,
            Name = name,
            HomologatedCode = homologatedCode,
            System = system,
            Status = status
        };

        return Result.Success(channel);
    }

    public void UpdateDetails(
        string newName,
        string newHomologatedCode,
        bool newSystem,
        string newStatus
    )
    {
        Name = newName;
        HomologatedCode = newHomologatedCode;
        System = newSystem;
        Status = newStatus;
    }
}