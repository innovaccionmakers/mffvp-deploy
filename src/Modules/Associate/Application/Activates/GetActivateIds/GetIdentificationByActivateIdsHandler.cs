using Associate.Domain.Activates;
using Associate.Integrations.Activates.GetActivateIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Associate.Application.Activates.GetActivateIds
{
    internal class GetIdentificationByActivateIdsHandler(
    IActivateRepository activateRepository) : IQueryHandler<GetIdentificationByActivateIdsRequestQuery, IReadOnlyCollection<GetIdentificationByActivateIdsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetIdentificationByActivateIdsResponse>>> Handle(
            GetIdentificationByActivateIdsRequestQuery query,
            CancellationToken cancellationToken)
        {
            var activate = await activateRepository.GetActivateByIdsAsync(query.ActivateIds, cancellationToken);

            var response = activate
            .Select(a => new GetIdentificationByActivateIdsResponse(
                a!.ActivateId,
                a.Identification))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetIdentificationByActivateIdsResponse>>(response);
        }
    }
}
