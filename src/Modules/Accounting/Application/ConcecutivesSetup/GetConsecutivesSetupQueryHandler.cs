using System.Linq;
using Accounting.Domain.Consecutives;
using Accounting.Integrations.ConsecutivesSetup;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.ConcecutivesSetup;

internal sealed class GetConsecutivesSetupQueryHandler(
    IConsecutiveRepository consecutiveRepository)
    : IQueryHandler<GetConsecutivesSetupQuery, IReadOnlyCollection<ConsecutiveSetupResponse>>
{
    public async Task<Result<IReadOnlyCollection<ConsecutiveSetupResponse>>> Handle(
        GetConsecutivesSetupQuery request,
        CancellationToken cancellationToken)
    {
        var consecutives = await consecutiveRepository.GetAllAsync(cancellationToken);

        var response = consecutives
            .Select(consecutive => new ConsecutiveSetupResponse(
                consecutive.ConsecutiveId,
                consecutive.Nature,
                consecutive.SourceDocument,
                consecutive.Number))
            .ToList();

        return Result.Success<IReadOnlyCollection<ConsecutiveSetupResponse>>(response);
    }
}
