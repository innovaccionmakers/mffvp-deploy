using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Integrations.Countries.GetCountry;
using People.Integrations.Countries;

namespace People.Application.Countries.GetCountry;

internal sealed class GetCountryQueryHandler(
    ICountryRepository countryRepository)
    : IQueryHandler<GetCountryQuery, CountryResponse>
{
    public async Task<Result<CountryResponse>> Handle(GetCountryQuery request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetAsync(request.CountryId, cancellationToken);
        if (country is null) return Result.Failure<CountryResponse>(CountryErrors.NotFound(request.CountryId));
        var response = new CountryResponse(
            country.CountryId,
            country.Name,
            country.ShortName,
            country.DaneCode,
            country.StandardCode
        );
        return response;
    }
}