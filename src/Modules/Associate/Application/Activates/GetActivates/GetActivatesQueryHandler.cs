using Associate.Domain.Activates;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.GetActivates;
using Associate.Integrations.ConfigurationParameters.GetConfigurationParameter;
using Associate.Integrations.ConfigurationParameters.GetConfigurationParameters;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Associate.Application.Activates.GetActivates;

internal sealed class GetActivatesQueryHandler(
    IActivateRepository activateRepository,
    ISender sender)
    : IQueryHandler<GetActivatesQuery, IReadOnlyCollection<ActivateResponse>>
{
    public async Task<Result<IReadOnlyCollection<ActivateResponse>>> Handle(GetActivatesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await activateRepository.GetAllAsync(cancellationToken);
        var listIdentification = entities.Select(x => x.DocumentType).Distinct().ToList();
        var documentType = new GetConfigurationParametersQuery();
        var configParametersResult = await sender.Send(documentType, cancellationToken);

        var guidToHomologationCode = configParametersResult.Value
            .Where(cp => listIdentification.Contains(cp.Uuid))
            .ToDictionary(cp => cp.Uuid, cp => cp.HomologationCode);

        var response = entities
            .Select(e => new ActivateResponse(
                e.ActivateId,
                guidToHomologationCode.TryGetValue(e.DocumentType, out var code) ? code : string.Empty,
                e.DocumentType,
                e.Identification,
                e.Pensioner,
                e.MeetsPensionRequirements,
                e.ActivateDate))
            .ToList();

        return Result.Success<IReadOnlyCollection<ActivateResponse>>(response);
    }
}