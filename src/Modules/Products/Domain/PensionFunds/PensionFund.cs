using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Interceptors;

using Products.Domain.Administrators;
using Products.Domain.PlanFunds;

namespace Products.Domain.PensionFunds;

public sealed class PensionFund : Entity, IHasRowVersion
{
    public int PensionFundId { get; private set; }
    public int IdentificationTypeId { get; private set; }
    public string Identification { get; private set; }
    public int Digit { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public Status Status { get; private set; }
    public string HomologatedCode { get; private set; }
    public int AdministratorId { get; private set; }
    public int BusinessCodeSfc { get; private set; }
    public long RowVersion { get; private set; }

    // Implementación explícita de la interfaz que expone el setter internamente
    long IHasRowVersion.RowVersion
    {
        get => RowVersion;
        set => RowVersion = value;
    }

    public Administrator Administrator { get; private set; } = null!;
    public IReadOnlyCollection<PlanFund> PlanFunds { get; private set; } = new List<PlanFund>();

    private PensionFund()
    {
    }

    public static Result<PensionFund> Create(
        int identificationTypeId,
        string identification,
        int digit,
        string name,
        string shortName,
        Status status,
        string homologatedCode,
        int administratorId
    )
    {
        var fund = new PensionFund
        {
            IdentificationTypeId = identificationTypeId,
            Identification = identification,
            Digit = digit,
            Name = name,
            ShortName = shortName,
            Status = status,
            HomologatedCode = homologatedCode,
            AdministratorId = administratorId
        };
        return Result.Success(fund);
    }

    public void UpdateDetails(
        int identificationTypeId,
        string identification,
        int digit,
        string name,
        string shortName,
        Status status,
        string homologatedCode,
        int administratorId
    )
    {
        IdentificationTypeId = identificationTypeId;
        Identification = identification;
        Digit = digit;
        Name = name;
        ShortName = shortName;
        Status = status;
        HomologatedCode = homologatedCode;
        AdministratorId = administratorId;
    }
}