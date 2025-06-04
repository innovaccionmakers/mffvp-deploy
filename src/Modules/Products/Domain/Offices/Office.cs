using Common.SharedKernel.Domain;

namespace Products.Domain.Offices;

public sealed class Office : Entity
{
    public int OfficeId { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public string Prefix { get; private set; }
    public string HomologatedCode { get; private set; }
    public int CityId { get; private set; }

    private Office()
    {
    }

    public static Result<Office> Create(
        string name,
        string status,
        string prefix,
        string homologatedCode,
        int cityId
    )
    {
        var office = new Office
        {
            Name = name,
            Status = status,
            Prefix = prefix,
            HomologatedCode = homologatedCode,
            CityId = cityId
        };

        office.Raise(new OfficeCreatedDomainEvent(office.OfficeId));
        return Result.Success(office);
    }

    public void UpdateDetails(
        string name,
        string status,
        string prefix,
        string homologatedCode,
        int cityId
    )
    {
        Name = name;
        Status = status;
        Prefix = prefix;
        HomologatedCode = homologatedCode;
        CityId = cityId;
    }
}