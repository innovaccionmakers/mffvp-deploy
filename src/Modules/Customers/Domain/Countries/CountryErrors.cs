using Common.SharedKernel.Domain;

namespace Customers.Domain.Countries;

public static class CountryErrors
{
    public static Error NotFound(int countryId)
    {
        return Error.NotFound(
            "Country.NotFound",
            $"The country with identifier {countryId} was not found"
        );
    }
}