using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Pipelines.Sockets.Unofficial.Arenas;
using Products.Domain.Alternatives;
using Products.Domain.Objectives;
using Products.Domain.Portfolios;
using Products.Integrations.Objectives.Queries;

namespace Products.Application.Objectives.Queries;

internal sealed class GetHomologateCodeByObjetiveIdQueryHandler(IObjectiveRepository objectiveRepository, IPortfolioRepository portfolioRepository) : IQueryHandler<GetHomologateCodeByObjetiveIdQuery, string>
{
    public async Task<Result<string>> Handle(GetHomologateCodeByObjetiveIdQuery request, CancellationToken cancellationToken)
    {                
        var objective = await objectiveRepository
           .GetByIdAsync(request.ObjetiveId, cancellationToken);

        var effectivePortfolioCode = string.Empty;

        if (objective is not null)
        {
            effectivePortfolioCode = await portfolioRepository.GetCollectorCodeAsync(
                objective.AlternativeId, cancellationToken);
        }

        return Result.Success<string>(effectivePortfolioCode ?? string.Empty);
    }
}