using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.Municipalities;
using Customers.Integrations.Municipalities.GetMunicipality;
using Customers.Integrations.Municipalities;

namespace Customers.Application.Municipalities.GetMunicipality;

internal sealed class GetMunicipalityQueryHandler(
    IMunicipalityRepository municipalityRepository)
    : IQueryHandler<GetMunicipalityQuery, MunicipalityResponse>
{
    public async Task<Result<MunicipalityResponse>> Handle(GetMunicipalityQuery request, CancellationToken cancellationToken)
    {
        var municipality = await municipalityRepository.GetAsync(request.HomologatedCode, cancellationToken);
        
        if (municipality is null)
            return null;

        var response = new MunicipalityResponse(
            municipality.MunicipalityId,
            municipality.CityCode,
            municipality.Name,
            municipality.DialingCode,
            municipality.DaneCode,
            municipality.HomologatedCode
        );
        return response;
    }
}