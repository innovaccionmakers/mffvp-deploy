using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Countries;
using Customers.Integrations.Countries.GetCountry;
using Customers.Integrations.Countries;

namespace Customers.Application.Countries.GetCountry;

internal sealed class GetCountryQueryHandler(
    ICountryRepository countryRepository)
    : IQueryHandler<GetCountryQuery, CountryResponse>
{

    public async Task<Result<CountryResponse>> Handle(GetCountryQuery request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetAsync(request.HomologatedCode, cancellationToken);

        if (country is null)
            return null;

        var response = new CountryResponse(
            country.CountryId,
            country.Name,
            country.ShortName,
            country.DaneCode,
            country.HomologatedCode
        );

        return response;
    }
}