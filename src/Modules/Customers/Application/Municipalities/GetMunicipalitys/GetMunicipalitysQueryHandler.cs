using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Municipalities;
using Customers.Integrations.Municipalities.GetMunicipalitys;
using Customers.Integrations.Municipalities;
using System.Collections.Generic;
using System.Linq;

namespace Customers.Application.Municipalities.GetMunicipalities;

internal sealed class GetMunicipalitysQueryHandler(
    IMunicipalityRepository municipalityRepository)
    : IQueryHandler<GetMunicipalitysQuery, IReadOnlyCollection<MunicipalityResponse>>
{
    public async Task<Result<IReadOnlyCollection<MunicipalityResponse>>> Handle(GetMunicipalitysQuery request, CancellationToken cancellationToken)
    {
        var entities = await municipalityRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new MunicipalityResponse(
                e.MunicipalityId,
                e.CityCode,
                e.Name,
                e.DialingCode,
                e.DaneCode,
                e.HomologatedCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<MunicipalityResponse>>(response);
    }
}