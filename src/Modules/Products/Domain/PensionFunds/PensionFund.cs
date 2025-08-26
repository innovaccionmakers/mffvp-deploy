using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.Domain.Administrators;
using Products.Domain.PlanFunds;

namespace Products.Domain.PensionFunds;

public sealed class PensionFund : Entity
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