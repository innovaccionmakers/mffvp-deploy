namespace Products.Presentation.GraphQL;

using HotChocolate;
using MediatR;
using Products.Integrations.Alternatives;
using Products.Integrations.ConfigurationParameters.DocumentTypes;
using Products.Integrations.ConfigurationParameters.GoalTypes;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Offices;
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

    public async Task<IReadOnlyCollection<GoalTypeDto>> GetGoalTypesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await mediator.Send(new GetGoalTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve goal types.");
        }

        var goalTypes = result.Value;

        return goalTypes.Select(x => new GoalTypeDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologationCode
        )).ToList();
    }


    public async Task<IReadOnlyCollection<GoalDto>> GetGoalsAsync(string typeId, string identification, StatusType status, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetObjectivesQuery(typeId, identification, status), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            var error = new GoalError(
                result.Error.Code,
                result.Error.Type.ToString(),
                result.Error.Description
            );

            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Description)
                    .SetCode(error.Code)
                    .SetExtension("type", error.Type)
                    .SetExtension("description", error.Description)
                    .Build()
            );
        }

        var goals = result.Value;

        return goals.Select(x => new GoalDto(
            x.Objective.ObjectiveId,
             x.Objective.ObjectiveName,
             x.Objective.Status
        )).ToList();
    }

    public async Task<IReadOnlyCollection<OfficeDto>> GetOfficesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOfficesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Failed to retrieve Offices")
                    .Build()
            );
        }

        var commercials = result.Value;

        return commercials.Select(x => new OfficeDto(
            x.OfficeId,
            x.Name,
            x.Prefix,
            x.CityId.ToString(),
            x.Status.ToString(),
            x.HomologatedCode
        )).ToList();

    }
    public async Task<IReadOnlyCollection<AlternativeDto>> GetAlternativesAsync(
        CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetAlternativesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve alternatives.");
        }

        var alternatives = result.Value;

        return alternatives.Select(x => new AlternativeDto(
            x.AlternativeId,
            x.AlternativeTypeId,
            x.Name,
            x.Description,
            x.Status.ToString(),
            x.HomologatedCode
        )).ToList();

    }
}
