using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Integrations.Countries.GetCountries;
using People.Integrations.Countries;
using System.Collections.Generic;
using System.Linq;

namespace People.Application.Countries.GetCountries;

internal sealed class GetCountriesQueryHandler(
    ICountryRepository countryRepository)
    : IQueryHandler<GetCountriesQuery, IReadOnlyCollection<CountryResponse>>
{
    public async Task<Result<IReadOnlyCollection<CountryResponse>>> Handle(GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await countryRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new CountryResponse(
                e.CountryId,
                e.Name,
                e.ShortName,
                e.DaneCode,
                e.StandardCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<CountryResponse>>(response);
    }
}