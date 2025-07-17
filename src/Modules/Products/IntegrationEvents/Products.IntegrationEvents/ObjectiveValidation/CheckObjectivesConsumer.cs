using Common.SharedKernel.Application.Rpc;
using Products.Domain.Objectives;

namespace Products.IntegrationEvents.ObjectiveValidation;

public sealed class CheckObjectivesConsumer : IRpcHandler<CheckObjectivesRequest, CheckObjectivesResponse>
{
    private readonly IObjectiveRepository _repository;

    public CheckObjectivesConsumer(IObjectiveRepository repository)
    {
        _repository = repository;
    }

    public async Task<CheckObjectivesResponse> HandleAsync(
        CheckObjectivesRequest message,
        CancellationToken cancellationToken)
    {
        var hasObjectives = await _repository.AnyAsync(message.AffiliateId, cancellationToken);

        return hasObjectives
            ? new CheckObjectivesResponse(true, null, null)
            : new CheckObjectivesResponse(false, "d8946440-0629-4284-9723-99bf7fd27067", "El Afiliado no tiene objetivos creados");
    }
}
