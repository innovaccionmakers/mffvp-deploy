using Common.SharedKernel.Domain;

namespace Products.Domain.Cities;

public sealed class City : Entity
{
    public int CityId { get; private set; }
    public string Name { get; private set; }

    private City()
    {
    }

    public static Result<City> Create(
        string name
    )
    {
        var city = new City
        {
            CityId = default,

            Name = name
        };

        city.Raise(new CityCreatedDomainEvent(city.CityId));
        return Result.Success(city);
    }

    public void UpdateDetails(
        string newName
    )
    {
        Name = newName;
    }
}