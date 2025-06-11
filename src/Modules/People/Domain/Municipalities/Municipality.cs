using Common.SharedKernel.Domain;

namespace People.Domain.Municipalities;

public sealed class Municipality : Entity
{
    public int MunicipalityId { get; private set; }
    public int CityCode { get; private set; }
    public string Name { get; private set; }
    public int DialingCode { get; private set; }
    public int DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private Municipality()
    {
    }

    public static Result<Municipality> Create(
        int cityCode,
        string name,
        int dialingCode,
        int daneCode,
        string homologatedCode)
    {
        var municipality = new Municipality
        {
            MunicipalityId = default,
            CityCode = cityCode,
            Name = name,
            DialingCode = dialingCode,
            DaneCode = daneCode,
            HomologatedCode = homologatedCode
        };

        return Result.Success(municipality);
    }

    public void UpdateDetails(
        int newCityCode,
        string newName,
        int newDialingCode,
        int newDaneCode,
        string newHomologatedCode)
    {
        CityCode = newCityCode;
        Name = newName;
        DialingCode = newDialingCode;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}