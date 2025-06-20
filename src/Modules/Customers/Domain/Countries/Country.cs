using Common.SharedKernel.Domain;

namespace Customers.Domain.Countries;
public sealed class Country : Entity
{
    public int CountryId { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public int DaneCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private Country() { }

    public static Result<Country> Create(
        string name, string shortname, int danecode, string homologatedcode
    )
    {
        var country = new Country
        {
                CountryId = new int(),
                Name = name,
                ShortName = shortname,
                DaneCode = danecode,
                HomologatedCode = homologatedcode,
        };
        country.Raise(new CountryCreatedDomainEvent(country.CountryId));
        return Result.Success(country);
    }

    public void UpdateDetails(
        string newName, string newShortName, int newDaneCode, string newHomologatedCode
    )
    {
        Name = newName;
        ShortName = newShortName;
        DaneCode = newDaneCode;
        HomologatedCode = newHomologatedCode;
    }
}