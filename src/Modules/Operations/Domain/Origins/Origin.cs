using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.OriginModes;

namespace Operations.Domain.Origins;

public sealed class Origin : Entity
{
    public int OriginId { get; private set; }
    public string Name { get; private set; }
    public bool OriginatorMandatory { get; private set; }
    public bool RequiresCertification { get; private set; }
    public bool RequiresContingentWithholding { get; private set; }
    public Status Status { get; private set; }
    public string HomologatedCode { get; private set; }
    
    private readonly List<OriginMode> _originModes = new();
    public IReadOnlyCollection<OriginMode> OriginModes => _originModes;

    private readonly List<AuxiliaryInformation> _auxiliaryInformations = new();
    public IReadOnlyCollection<AuxiliaryInformation> AuxiliaryInformations => _auxiliaryInformations;

    private Origin()
    {
    }

    public static Result<Origin> Create(
        string name,
        bool originatorMandatory,
        bool requiresCertification,
        bool requiresContingentWithholding,
        Status status,
        string homologatedCode
    )
    {
        var origin = new Origin
        {
            OriginId = default,
            Name = name,
            OriginatorMandatory = originatorMandatory,
            RequiresCertification = requiresCertification,
            RequiresContingentWithholding = requiresContingentWithholding,
            Status = status,
            HomologatedCode = homologatedCode
        };
        return Result.Success(origin);
    }

    public void UpdateDetails(
        string newName,
        bool newOriginatorMandatory,
        bool newRequiresCertification,
        bool newRequiresContingentWithholding,
        Status newStatus,
        string newHomologatedCode
    )
    {
        Name = newName;
        OriginatorMandatory = newOriginatorMandatory;
        RequiresCertification = newRequiresCertification;
        RequiresContingentWithholding = newRequiresContingentWithholding;
        Status = newStatus;
        HomologatedCode = newHomologatedCode;
    }
}