using Common.SharedKernel.Domain;

namespace Customers.Domain.Municipalities;
public sealed class Municipality : Entity
{
    public int MunicipalityId { get; private set; }
    public int CityCode { get; private set; }
    public string Name { get; private set; }
    public int DialingCode { get; private set; }
    public int DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private Municipality() { }

    public static Result<Municipality> Create(
        int citycode, string name, int dialingcode, int danecode, string homologatedcode
    )
    {
        var municipality = new Municipality
        {
                MunicipalityId = new int(),
                CityCode = citycode,
                Name = name,
                DialingCode = dialingcode,
                DaneCode = danecode,
                HomologatedCode = homologatedcode,
        };
        municipality.Raise(new MunicipalityCreatedDomainEvent(municipality.MunicipalityId));
        return Result.Success(municipality);
    }

    public void UpdateDetails(
        int newCityCode, string newName, int newDialingCode, int newDaneCode, string newHomologatedCode
    )
    {
        CityCode = newCityCode;
        Name = newName;
        DialingCode = newDialingCode;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}