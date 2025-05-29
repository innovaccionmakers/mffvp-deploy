using Common.SharedKernel.Domain;

namespace Products.Domain.Cities;

public static class CityErrors
{
    public static Error NotFound(int cityId)
    {
        return Error.NotFound(
            "City.NotFound",
            $"The city with identifier {cityId} was not found"
        );
    }
}