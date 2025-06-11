using Common.SharedKernel.Domain;

namespace People.Domain.Countries;

public sealed class Country : Entity
{
    public int CountryId { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public string DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private Country()
    {
    }

    public static Result<Country> Create(
        string name,
        string shortName,
        string daneCode,
        string homologatedCode)
    {
        var country = new Country
        {
            CountryId = default,
            Name = name,
            ShortName = shortName,
            DaneCode = daneCode,
            HomologatedCode = homologatedCode
        };

        country.Raise(new CountryCreatedDomainEvent(country.CountryId));
        return Result.Success(country);
    }

    public void UpdateDetails(
        string newName,
        string newShortName,
        string newDaneCode,
        string newHomologatedCode)
    {
        Name = newName;
        ShortName = newShortName;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}