using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Interceptors;

using Products.Domain.PensionFunds;

namespace Products.Domain.Administrators;

public sealed class Administrator : Entity, IHasRowVersion
{
    public int AdministratorId { get; private set; }
    public string Identification { get; private set; }
    public int IdentificationTypeId { get; private set; }
    public int Digit { get; private set; }
    public string Name { get; private set; }
    public Status Status { get; private set; }
    public string EntityCode { get; private set; }
    public int EntityType { get; private set; }
    public string SfcEntityCode { get; private set; }
    public long RowVersion { get; private set; }

    // Implementación explícita de la interfaz que expone el setter internamente
    long IHasRowVersion.RowVersion
    {
        get => RowVersion;
        set => RowVersion = value;
    }

    public IReadOnlyCollection<PensionFund> PensionFunds { get; private set; } = new List<PensionFund>();

    private Administrator()
    {
    }

    public static Result<Administrator> Create(
        string identification,
        int identificationTypeId,
        string name,
        Status status,
        string entityCode,
        int digit,
        int entityType,
        string sfcEntityCode)
    {
        var administrator = new Administrator
        {
            Identification = identification,
            IdentificationTypeId = identificationTypeId,
            Name = name,
            Status = status,
            EntityCode = entityCode,
            Digit = digit,
            EntityType = entityType,
            SfcEntityCode = sfcEntityCode
        };

        return Result.Success(administrator);
    }

    public void UpdateDetails(
        string identification,
        int identificationTypeId,
        string name,
        Status status,
        string entityCode,
        int digit,
        int entityType,
        string sfcEntityCode)
    {
        Identification = identification;
        IdentificationTypeId = identificationTypeId;
        Name = name;
        Status = status;
        EntityCode = entityCode;
        Digit = digit;
        EntityType = entityType;
        SfcEntityCode = sfcEntityCode;
    }
}