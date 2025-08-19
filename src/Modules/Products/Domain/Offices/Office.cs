using Common.SharedKernel.Domain;

namespace Products.Domain.Offices;

public sealed class Office : Entity
{
    public int OfficeId { get; private set; }
    public string Name { get; private set; }
    public Status Status { get; private set; }
    public string Prefix { get; private set; }
    public string HomologatedCode { get; private set; }
    public int CityId { get; private set; }
    public string CostCenter { get; private set; }

    private Office()
    {
    }

    public static Result<Office> Create(
        string name,
        Status status,
        string prefix,
        string homologatedCode,
        int cityId,
        string costCenter
    )
    {
        var office = new Office
        {
            Name = name,
            Status = status,
            Prefix = prefix,
            HomologatedCode = homologatedCode,
            CityId = cityId,
            CostCenter = costCenter
        };

        office.Raise(new OfficeCreatedDomainEvent(office.OfficeId));
        return Result.Success(office);
    }

    public void UpdateDetails(
        string name,
        Status status,
        string prefix,
        string homologatedCode,
        int cityId,
        string costCenter
    )
    {
        Name = name;
        Status = status;
        Prefix = prefix;
        HomologatedCode = homologatedCode;
        CityId = cityId;
        CostCenter = costCenter;
    }
}