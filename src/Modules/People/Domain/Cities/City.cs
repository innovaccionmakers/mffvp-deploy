using Common.SharedKernel.Domain;

namespace People.Domain.Cities;

public sealed class City : Entity
{
    public int CityId { get; private set; }
    public string Name { get; private set; }

    private City()
    {
    }

    public static Result<City> Create(string name)
    {
        var city = new City
        {
            CityId = default,
            Name = name
        };

        return Result.Success(city);
    }

    public void UpdateDetails(string newName)
    {
        Name = newName;
    }
}