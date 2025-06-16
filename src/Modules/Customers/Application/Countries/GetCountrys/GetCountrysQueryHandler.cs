using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Countries;
using Customers.Integrations.Countries.GetCountrys;
using Customers.Integrations.Countries;
using System.Collections.Generic;
using System.Linq;

namespace Customers.Application.Countries.GetCountries;

internal sealed class GetCountrysQueryHandler(
    ICountryRepository countryRepository)
    : IQueryHandler<GetCountrysQuery, IReadOnlyCollection<CountryResponse>>
{
    public async Task<Result<IReadOnlyCollection<CountryResponse>>> Handle(GetCountrysQuery request, CancellationToken cancellationToken)
    {
        var entities = await countryRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new CountryResponse(
                e.CountryId,
                e.Name,
                e.ShortName,
                e.DaneCode,
                e.HomologatedCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<CountryResponse>>(response);
    }
}