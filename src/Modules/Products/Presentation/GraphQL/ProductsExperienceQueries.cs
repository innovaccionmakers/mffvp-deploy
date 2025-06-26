namespace Products.Presentation.GraphQL;

using MediatR;
using Products.Integrations.ConfigurationParameters.DocumentTypes;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Portfolios.Queries;
using Products.Presentation.DTOs;


public class ProductsExperienceQueries(IMediator mediator) : IProductsExperienceQueries
{
    public async Task<PortfolioDto> GetPortfolioAsync(string objetiveId,
        CancellationToken cancellationToken)
    {
            var result = await mediator.Send(new GetPortfolioInformationByObjetiveIdQuery(objetiveId), cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                throw new InvalidOperationException("Failed to retrieve portfolio information.");
            }

            var portfolioInformation = result.Value;


            return new PortfolioDto(
                portfolioInformation.Found,
                portfolioInformation.Plan,
                portfolioInformation.Alternative,
                portfolioInformation.Portfolio
            );
    }

    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypesAsync(
      CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetDocumentTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction types.");
        }

        var documentTypes = result.Value;


        return documentTypes.Select(x => new DocumentTypeDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }


    public async Task<IReadOnlyCollection<GoalDto>> GetGoalsAsync(string typeId, string identification, StatusType status, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetObjectivesQuery(typeId, identification, status), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve goals.");
        }

        var goals = result.Value;

        return goals.Select(x => new GoalDto(
            x.Objective.ObjectiveId,
             x.Objective.ObjectiveName,
             x.Objective.Status
        )).ToList();
    }
}
